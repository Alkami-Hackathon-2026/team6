using Alkami.Broker.MessageTemplates;
using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Exceptions;
using Alkami.MicroServices.Settings.Contracts;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Data;
using Alkami.MicroServices.Settings.Service.Client;
using Alkami.Services.Subscriptions.ParticipatingClient;
using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    /// <summary>
    /// Provides access to all active endpoints that support the same provider type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProviderBasedClient<T> : SelfResolvingClient<T> where T : class
    {
        #region ClassObjects

        public static Func<ISettingsServiceContract> SettingsClientFactory = () => new ServiceClient();
        public static IAutoReset AutoResetHandle
        {
            get { return _autoResetHandle; }
            set
            {
                _autoResetHandle = value;
                _autoResetHandle.TimeoutEvent += HandleCacheTimeout;
            }
        }

        private const string RefreshIntervalSecondsSettingName = "ProviderBasedClientCacheRefreshIntervalSeconds";
        private const int DefaultIntervalTimeoutSecondsValue = 600; //10 minutes
        private static readonly ILog _logger
            = LogManager.GetLogger("Alkami.MicroServices.Subscriptions.ProviderBased.ProviderBasedSelfResolvingClient");
        private static readonly ConcurrentDictionary<Guid, ProviderCacheContainer> _bankProviders
            = new ConcurrentDictionary<Guid, ProviderCacheContainer>();
        private static readonly ConcurrentDictionary<Guid, object> _bankLocks
            = new ConcurrentDictionary<Guid, object>();
        private static string _providerType;
        private static readonly List<string> ValidItemTypes = new List<string>() { "Connector", "Processor" };
        private static IAutoReset _autoResetHandle;
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        private readonly long? _providerId;
        private readonly string _providerName;

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor for generic class
        /// </summary>
        static ProviderBasedClient()
        {
            //Events are still important even with cache refreshes because the cache refresh is configurable and could be 
            //high enough for a more immediate response to be required
            Broker.App.Subscription.Add(Broker.App.Events.CacheRemoved, dict =>
            {
                var cacheArgs = dict.ConvertArgsTo<CacheExpiredArgs>();

                HandleCacheEvent(cacheArgs);
            });

            AutoResetHandle = new AutoReset(DefaultIntervalTimeoutSecondsValue, RefreshIntervalSecondsSettingName);
        }

        /// <summary>
        /// Instantiates an instance of the class
        /// </summary>
        /// <param name="providerType">is static for the generic class</param>
        /// <param name="providerId"></param>
        public ProviderBasedClient(string providerType, long providerId)
            : this(providerType)
        {
            if (providerId <= 0)
            {
                throw new ArgumentNullException("providerId is not valid");
            }

            _providerId = providerId;
        }

        /// <summary>
        /// Instantiates an instance of the class
        /// </summary>
        /// <param name="providerType">is static for the generic class</param>
        /// <param name="providerName"></param>
        public ProviderBasedClient(string providerType, string providerName)
            : this(providerType)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentNullException("providerName cannot be null");
            }

            _providerName = providerName;
        }

        /// <summary>
        /// Instantiates an instance of the class
        /// </summary>
        /// <param name="providerType">is static for the generic class</param>
        public ProviderBasedClient(string providerType)
        {
            if (string.IsNullOrWhiteSpace(providerType))
            {
                throw new ArgumentNullException("providerType cannot be null");
            }

            // The type should never change, however preventing that here could break tests in other repositories
            _providerType = providerType;
        }

        #endregion

        #region PublicAndOriginalMethod

        /// <summary>
        /// Executes the provided function safely to ensure all clients are properly released.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="operation">The operation to call on the client.</param>
        /// <param name="request">The instance of the request object to pass to the operation.</param>
        /// <param name="callingMethod">The name of the method calling this method.</param>
        /// <returns>The response of the operation from multiple clients.</returns>
        protected Task<TResponse>[] ProxyCallMultiple<TRequest, TResponse>(Func<T, TRequest, Task<TResponse>> operation, TRequest request, [CallerMemberName] string callingMethod = null)
            where TRequest : BaseRequest
            where TResponse : BaseResponse, new()
        {
            var providers = this.Client(request);
            var tasks = new List<Task<TResponse>>();
            var firstGroup = true;

            foreach (var providerGroup in providers.GroupBy(x => x.ClaimsIdentitySerializationMethod))
            {
                // For the first serialization method, just use the original request.
                var requestForGroup = (firstGroup) ? request : (TRequest)request.ShallowClone();

                // Set the serialization method, which will ensure to clear the SUT if needed.
                requestForGroup.ClaimsIdentitySerializationMethod = providerGroup.Key;

                foreach (var providerServiceData in providerGroup)
                {
                    tasks.Add(this.ProxyCallInternal(operation, providerServiceData.Client, requestForGroup, callingMethod));
                }

                firstGroup = false;
            }

            return tasks.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal List<ProviderServiceData<T>> Client(BaseRequest request)
        {
            try
            {
                if (!request.BankIdentifier.HasValue)
                {
                    throw new ArgumentNullException("BankIdentifier cannot be null");
                }

                var providers = GetProviders(request);
                var clients = LoadServiceDefinitions(request, providers);

                if (!clients.Any())
                {
                    throw new Exception("No service definition(s) be found for the provider type providers");
                }

                return clients;
            }
            catch (Exception exception)
            {
                var exceptionMessage = $"Could not find a compatible service for {typeof(T).FullName}. {exception.Message}";

                _logger.Error(exceptionMessage);

                throw new ServiceInstanceNotFoundException(exceptionMessage);
            }
        }

        private List<ProviderServiceData<T>> LoadServiceDefinitions(BaseRequest request, List<ProviderDefinition> providers)
        {
            var clients = new List<ProviderServiceData<T>>();
            var favoredSerializationMethod = GetFavoredClaimsIdentitySerializationMethod();

            foreach (var providerNameGroup in providers.GroupBy(x => x.Name))
            {
                var list = ServiceEndpoints
                    .Where(x => x.ServiceDefinition.ProviderConfiguration != null
                        && providerNameGroup.Key.Equals(x.ServiceDefinition.ProviderConfiguration.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.Trace(i => i($"Found {list.Count} service definition(s) for provider {providerNameGroup.Key} out of {ServiceEndpoints.Count}"));

                var innerKewlLoadbalancing = new BestAverageThroughputLoadBalancer<T>(list);
                var definition = innerKewlLoadbalancing.GetNext(request);

                if (definition == null)
                {
                    _logger.Warn(i => i($"Could not find a compatible service for {typeof(T).FullName}, with name {providerNameGroup.Key}"));
                }
                else
                {
                    clients.Add(new ProviderServiceData<T>()
                    {
                        ClaimsIdentitySerializationMethod = DetermineClaimsIdentitySerializationMethod(definition.ServiceDefinition, favoredSerializationMethod),
                        Client = definition.GetClient(),
                    });
                }
            }

            return clients;
        }

        private static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        private static void RunSync(Func<Task> func)
        {
            _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        #endregion

        #region ProviderDataAccess

        /// <summary>
        /// Returns provider definitions matching the provided arguments. Will attempt to retrieve providers from the Settings service if not already in cache.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal List<ProviderDefinition> GetProviders(BaseRequest request)
        {
            if (!_bankProviders.ContainsKey(request.BankIdentifier.Value))
            {
                GetProvidersAndUpdateCache(request);
            }

            if (_bankProviders.ContainsKey(request.BankIdentifier.Value))
            {
                var providers = _bankProviders[request.BankIdentifier.Value]
                    .Providers
                    .Select(p => p.Value)
                    .ToList();

                if (_providerId.HasValue)
                {
                    _logger.Debug(i => i($"Checking provider list for provider id {_providerId.Value}"));

                    providers = providers.Where(x => x.Id == _providerId).ToList();
                }
                else if (!string.IsNullOrEmpty(_providerName))
                {
                    _logger.Debug(i => i($"Checking provider list for provider name {_providerName}"));

                    providers = providers.Where(x => x.Name.Equals(_providerName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (providers.Count > 1)
                    {
                        _logger.Warn(i => i($"More than one provider found matching criteria: type:{_providerType}, name:{_providerName}"));
                    }
                }

                _logger.Trace(i => i($"Found these providers for client instance: {string.Join(", ", providers.Select(x => x.Name))}"));

                return providers;
            }

            return new List<ProviderDefinition>();
        }

        private static List<ProviderDefinition> GetProviderCacheItems(BaseRequest request)
        {
            var providerCacheItems = new List<ProviderDefinition>();
            var providers = GetProvidersFromService(request);
            var items = GetProviderItems(providers.Select(p => p.Id).ToList(), request);

            foreach (var provider in providers)
            {
                if (items.Any(i => i.ParentId == provider.Id))
                {
                    var namePartsArray = provider.Name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                    var providerDefinition = new ProviderDefinition()
                    {
                        Id = provider.Id,
                        Name = namePartsArray.Last().Trim(),
                        RootName = namePartsArray.Length > 1 ? namePartsArray.First().Trim() : null,
                        ProviderTypeName = _providerType
                    };

                    providerCacheItems.Add(providerDefinition);
                }
                else
                {
                    _logger.Warn(i => i($"No valid item could be found for provider id: {provider.Id}, provider type {_providerType}"));
                }
            }

            return providerCacheItems;
        }

        private static List<Provider> GetProvidersFromService(BaseRequest request)
        {
            var getproviderRequest = new GetProviderTypeRequest();
            getproviderRequest.CopyBaseFrom(request);

            // The following two lines were added because many BaseRequest objects are setting MaxResults to 1 for what the initial caller needs.
            // We don't want to enforce that restriction when looking up providers as there could be multiple and CopyBaseFrom can restrict that
            // unintentionally. So we change the values here to values that will not restrict the lookup for GetProviderType and GetItems further down.
            getproviderRequest.MaxResults = 1000;
            getproviderRequest.Page = 0;
            getproviderRequest.Filter = new ProviderTypeFilter
            {
                PartialName = _providerType
            };
            getproviderRequest.Mapping = new ProviderTypeMapper()
            {
                ShouldIncludeProviders = true
            };

            var response = RunSync(() => SettingsClientFactory().GetProviderTypeAsync(getproviderRequest));

            MicroserviceHelper.CheckForError(response);

            var pt = response.ProviderTypes.FirstOrDefault(p => p.Name.Equals(_providerType, StringComparison.InvariantCultureIgnoreCase));

            if (pt == null || pt.Providers == null || !pt.Providers.Any())
            {
                throw new AlkamiException(new InvalidDataException($"No providers were found for provider type: {_providerType}"),
                    ErrorCode.Informational, SubCode.None, "No providers found");
            }

            _logger.Trace(i => i($"Providers returned from service for {_providerType}: {string.Join(",", pt.Providers.Select(x => x.Name))}"));

            return pt.Providers;
        }

        private static List<Item> GetProviderItems(List<long> providerIds, BaseRequest request)
        {
            var itemRequest = new GetItemRequest();
            itemRequest.CopyBaseFrom(request);
            itemRequest.MaxResults = 1000;
            itemRequest.Page = 0;
            itemRequest.Filter = new ItemFilter
            {
                ParentIds = providerIds,
                ItemTypes = ValidItemTypes
            };

            var itemResponse = RunSync(() => SettingsClientFactory().GetItemsAsync(itemRequest));

            MicroserviceHelper.CheckForError(itemResponse);

            return itemResponse.Items.Where(i => !i.Deleted).ToList();
        }

        #endregion

        #region CacheManagement

        /// <summary>
        /// Intended for testing only
        /// </summary>
        internal static void ClearCache()
        {
            _bankProviders.Clear();
        }

        internal static void HandleCacheEvent(CacheExpiredArgs cacheArgs)
        {
            if (cacheArgs.CacheKey.IndexOf("Provider", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                if (Guid.TryParse(cacheArgs[Broker.Base.ReservedKeyNames.BankIdentifier], out Guid bank)
                        && _bankProviders.ContainsKey(bank))
                {
                    _bankProviders.TryRemove(bank, out ProviderCacheContainer removedCacheItem);
                }
            }
            else if (cacheArgs.CacheKey.Split('|').Count() == 5)
            {
                var split = cacheArgs.CacheKey.Split('|'); //[string.Format("{0}|{1}|0|{2}|{3}", key, itemType, parentId, secondaryId);]
                var itemType = split[1];
                var providerIdString = split[3];

                if ((itemType.Equals("Connector", StringComparison.CurrentCultureIgnoreCase)
                        || itemType.Equals("Processor", StringComparison.CurrentCultureIgnoreCase))
                            && long.TryParse(providerIdString, out long providerId)
                            && Guid.TryParse(cacheArgs[Broker.Base.ReservedKeyNames.BankIdentifier], out Guid bank)
                            && _bankProviders.ContainsKey(bank)
                            && _bankProviders[bank].Providers.Any(x => x.Key == providerId))
                {
                    _bankProviders.TryRemove(bank, out ProviderCacheContainer removedCacheItem);
                }
            }
        }

        private static void HandleCacheTimeout(object state, EventArgs e)
        {
            RefreshCache();
        }

        private static void RefreshCache()
        {
            _logger.Debug(i => i($"Entering RefreshCache for providerType: {_providerType}"));

            var bankIdentifiers = _bankProviders.Keys.ToArray();
            var tasks = new List<Task>();

            for (int i = 0; i < bankIdentifiers.Length; i++)
            {
                tasks.Add(RefreshProviderCacheAsync(bankIdentifiers[i]));
            }

            if (tasks.Any())
            {
                RunSync(() => Task.WhenAll(tasks));
            }
        }

        private static Task RefreshProviderCacheAsync(Guid bankIdentifier)
        {
            return Task.Run(() =>
            {
                try
                {
                    //If this lack of claims ever becomes an issue for the settings service then we may need to get and cache admin claims
                    var getproviderRequest = new GetProviderTypeRequest();
                    getproviderRequest.BankIdentifier = bankIdentifier;

                    _logger.Debug(i => i($"RefreshProviderCacheAsync: bank: {bankIdentifier}, providerType: {_providerType}"));

                    GetProvidersAndUpdateCache(getproviderRequest);
                }
                catch (Exception exception)
                {
                    _logger.Error($"An exception occurred while refreshing the provider cache for: "
                        + $"bank - {bankIdentifier}, provider type - {_providerType}. {exception.Message}");
                }
            });
        }

        private static void GetProvidersAndUpdateCache(BaseRequest request)
        {
            if (!_bankLocks.ContainsKey(request.BankIdentifier.Value))
            {
                _bankLocks.TryAdd(request.BankIdentifier.Value, new object());
            }

            lock (_bankLocks[request.BankIdentifier.Value])
            {
                if (!_bankProviders.ContainsKey(request.BankIdentifier.Value))
                {
                    _logger.Debug(i => i($"GetProvidersAndUpdateCache: adding cache for bank: {request.BankIdentifier.Value}, providerType: {_providerType}"));

                    var providers = GetProviderCacheItems(request);

                    AddOrUpdateProvidersInCache(request.BankIdentifier.Value, providers);

                    //we only need to start the initial auto reset when there is something to refresh
                    AutoResetHandle.Start();
                } //we will allow a refresh no more often than half of the time of the scheduled refresh interval
                else if (DateTime.Now.Subtract(_bankProviders[request.BankIdentifier.Value].LastRefreshedDateTime).TotalSeconds >= (AutoResetHandle.TimeoutSeconds / 2))
                {
                    _bankProviders[request.BankIdentifier.Value].LastRefreshedDateTime = DateTime.Now;

                    _logger.Debug(i => i($"GetProvidersAndUpdateCache: seeking to update cache for bank: {request.BankIdentifier.Value}, providerType: {_providerType}"));

                    var providers = GetProviderCacheItems(request);

                    AddOrUpdateProvidersInCache(request.BankIdentifier.Value, providers);
                }
            }
        }

        private static void AddOrUpdateProvidersInCache(Guid bankIdentifier, List<ProviderDefinition> providers)
        {
            if (!_bankProviders.ContainsKey(bankIdentifier))
            {
                var providersContainer = new ProviderCacheContainer();
                providers.ForEach(p => providersContainer.Providers.TryAdd(p.Id, p));

                _bankProviders.TryAdd(bankIdentifier, providersContainer);
            }
            else
            {
                var cachedProviders = _bankProviders[bankIdentifier]
                    .Providers
                    .Select(p => p.Value);

                foreach (var provider in providers.Where(x => !cachedProviders.Any(p => p.Id == x.Id)))
                {
                    _logger.Debug(i => i($"AddOrUpdateProvidersInCache: adding new provider to cache. bank: {bankIdentifier}, providerId: {provider.Id}"));
                    _bankProviders[bankIdentifier].Providers.TryAdd(provider.Id, provider);
                }

                foreach (var provider in cachedProviders.Where(x => !providers.Any(p => p.Id == x.Id)).ToList())
                {
                    _logger.Debug(i => i($"AddOrUpdateProvidersInCache: removing provider from cache. bank: {bankIdentifier}, providerId: {provider.Id}"));
                    _bankProviders[bankIdentifier].Providers.TryRemove(provider.Id, out ProviderDefinition removedProvider);
                }
            }
        }

        #endregion
    }
}