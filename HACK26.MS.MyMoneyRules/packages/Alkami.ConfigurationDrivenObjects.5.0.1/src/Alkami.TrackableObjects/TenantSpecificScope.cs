using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Data;
using Alkami.MicroServices.Settings.Service.Client;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Alkami.Data.Access;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using Alkami.TrackableObjects.Exceptions;
using SettingDescriptor = Alkami.MicroServices.Settings.ProviderBased.Contracts.SettingDescriptor;

namespace Alkami.TrackableObjects
{
    public class TenantSpecificScope : IDisposable
    {
        /// <summary>
        /// A factory method to create the utility class...
        /// </summary>
        public static Func<ISettingsServiceContract> ServiceContractFactory = () => new ServiceClient();

        //A way to disabled caching. This allows setting to be retrieved from the service every time. Useful if unit testing is occurring. 
        public static bool CacheItems = true;

        private static readonly ILog Logger = LogManager.GetLogger<TenantSpecificScope>();
        public Item Instance { get { return this._instance; }}
        private Item _instance;
        private readonly ConfigurationDrivenObject _parent;
        private readonly BaseRequest _parentRequest;
        private Settings _settings;
        private bool _scopeCreated;

        public long ItemId { get { return _instance.Id; } }

        public long ParentId { get { return _instance.ParentId; } }

        public string Name { get; private set; }

        public long SecondaryId { get { return _instance.SecondaryId; } }

        public bool ScopeCreated => _scopeCreated;
        public Settings Settings => _settings;

        public TenantSpecificScope(ConfigurationDrivenObject parent, BaseRequest request)
        {
            _parent = parent;
            _parentRequest = request;
        }

        public async Task<TenantSpecificScope> CreateScopeAsync(string name = null)
        {
            _scopeCreated = false;
            //if (_parentRequest.BankIdentifier == null || _parentRequest.BankIdentifier == Guid.Empty)
            //{
            //    var tenant = DataScope.GetCurrentTenant(_parentRequest);
            //    _parentRequest.SetBankIdentifier(tenant);
            //}

            var itemStoreCacheKey = new ItemStoreCacheKey
            {
                BankIdentifier = _parentRequest.BankIdentifier.GetValueOrDefault(),
                ItemType = _parent.ItemType,
                ItemName = name ?? _parent.Name
            };

            if (CacheItems)
            {
                _instance = ItemStore.FromCache(itemStoreCacheKey);
            }

            if (_instance == null)
            {
                var itemRequest = new GetItemRequest()
                {
                    Filter = await _parent.GetFilterAsync(_parentRequest, name ?? _parent.Name),
                    Mapping = new ItemMapper
                    {
                        ShouldIncludeSettings = true
                    }
                };

                itemRequest.CopyBaseFrom(_parentRequest);

                var result = await ServiceContractFactory().GetItemsAsync(itemRequest);

                if (result.HasError)
                    throw new Exception(result.SystemMessage);

                if (result.Items?.Count() > 1)
                    throw new AmbiguousMatchException("Incorrect number of results returned");

                var notDeleted = result.Items?.Where(x => !x.Deleted);
               // var notDeleted = result.Items;
                if (notDeleted == null || !notDeleted.Any())
                {
                    Logger.Debug(x => x($"No providers returned for key: {itemStoreCacheKey.ToString()}"));
                    _instance = new Item() { ItemSettings = new List<ItemSetting>() };
                }
                else
                {
                    _instance = notDeleted?.FirstOrDefault() ?? new Item() { ItemSettings = new List<ItemSetting>() };
                }

                if (CacheItems)
                {
                    ItemStore.AddToCache(itemStoreCacheKey, _instance);
                }
            }

            _scopeCreated = _instance.Id == 0 ? false : true;

            _settings = new Settings(_parent, _instance);
         

            var parts = itemStoreCacheKey.ItemName.Split(new[] {":: "}, StringSplitOptions.RemoveEmptyEntries);

            this.Name = parts.Length > 0 ? parts.First().Trim() : itemStoreCacheKey.ItemName;

            return this;
        }


        public void Dispose()
        {
        }

        ///  <summary>
        /// GetSettingOrDefault
        ///  </summary>
        ///  <typeparam name="T"></typeparam>
        ///  <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSettingOrDefault<T>(string name, T defaultValue = default(T))
        {
            if (!_scopeCreated)
            {
                throw new Exception("Scope not Created!");
            }

            return _settings.GetSettingOrDefault(name, defaultValue);
        }

        public T GetSettingOrDefault2<T>(string name) => GetSettingOrDefault<T>(name);

        public async Task<UserSettings> GetUserSettingsAsync(long userId)
        {
            if (!_scopeCreated)
            {
                throw new Exception("Scope not Created!");
            }

            var userSettings = new UserSettings(_parent, this, userId);

            return await userSettings.CreateScopeAsync(_parentRequest);
        }

        public class UserSettings : ConfigurationDrivenObject, IDisposable
        {
            private readonly ConfigurationDrivenObject _parent;
            private readonly TenantSpecificScope _parentScope;
            private readonly long _userId;
            private Item _item;

            protected internal UserSettings(ConfigurationDrivenObject parent, TenantSpecificScope parentScope, long userId)
            {
                _parent = parent;
                _userId = userId;
                _parentScope = parentScope;
            }

            internal async Task<UserSettings> CreateScopeAsync(BaseRequest request)
            {
                var itemRequest = new GetItemRequest()
                {
                    Filter = new ItemFilter()
                    {
                        ItemType = ItemType,
                        ParentIds = new List<long>() { _parentScope.ItemId },
                        SecondaryIds = new List<long>() { _userId }
                    },
                    Mapping = new ItemMapper
                    {
                        ShouldIncludeSettings = true
                    }
                };

                itemRequest.CopyBaseFrom(request);

                var result = await ServiceContractFactory().GetItemsAsync(itemRequest);

                if (result.HasError)
                    throw new Exception(result.SystemMessage);
                if (result.Items.Count > 1)
                    throw new AmbiguousMatchException("Too many results returned");

                _item = result.Items.FirstOrDefault() ?? new Item()
                {
                    Name = ItemType,
                    ItemType = ItemType,
                    ParentId = _parentScope.ItemId,
                    SecondaryId = _userId,
                    CreatedUtc = DateTime.UtcNow,
                    Version = "1.0.0.0",
                    ItemSettings = new List<ItemSetting>()
                };
                InnerSettings = new Settings(this, _item);

                return this;
            }

            public override string ItemType
            {
                get { return string.Format("{0}User", _parent.ItemType); }
            }

            private Settings InnerSettings { get; set; }

            public void AddOrUpdateSetting<T>(string name, T value)
            {
                lock (_item)
                {
                    var setting = _item.ItemSettings.FirstOrDefault(x => x.Name == name);
                    if (setting == null)
                    {
                        setting = new ItemSetting()
                        {
                            CreatedUtc = DateTime.UtcNow,
                            Name = name,
                            Value = value.ToString(),
                            Version = _item.Version,
                            ItemId = _item.Id
                        };
                        _item.ItemSettings.Add(setting);
                    }
                    setting.Value = value.ToString();
                    InnerSettings = new Settings(this, _item);
                }
            }

            public async Task CommitAsync(string commitMessage)
            {
                var addOrUpdateItemRequest = new AddOrUpdateItemRequest
                {
                    ItemList = new List<Item>()
                        {
                            _item
                        },
                    CommitMessage = commitMessage
                };
                addOrUpdateItemRequest.CopyBaseFrom(_parentScope._parentRequest);

                var updateItemsResponse = await ServiceContractFactory().AddOrUpdateItemsAsync(addOrUpdateItemRequest);

                if (updateItemsResponse.HasError)
                {
                    var errors = updateItemsResponse.ValidationResults.Select(x => x.Message);

                    Logger.DebugFormat("Upserting an Item's details failed, for the following reasons: {0}", string.Join(",", errors));

                    throw new ItemUpdateFailedException(_item, updateItemsResponse.ValidationResults);
                }
            }

            public void Dispose()
            {
                // should we commit here or rollback if nothing is done?
            }


            ///  <summary>
            /// GetSettingOrDefault
            ///  </summary>
            ///  <typeparam name="T"></typeparam>
            ///  <param name="name"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public T GetSettingOrDefault<T>(string name, T defaultValue = default(T))
            {
                return InnerSettings.GetSettingOrDefault(name, defaultValue);
            }

            public override List<SettingDescriptor> SettingDescriptors()
            {
                return new List<SettingDescriptor>();
            }

            public override Dictionary<string, string> DefaultSettings()
            {
                return new Dictionary<string, string>();
            }

            protected override Task<long> GetParentIdAsync(BaseRequest parentRequest)
            {
                return Task.FromResult(_parentScope.ItemId);
            }

            protected override Task<long> GetSecondaryIdAsync(BaseRequest parentRequest)
            {
                return Task.FromResult(_userId);
            }

            protected override void ValidateChangedSetting(SettingDescriptor settingDescriptor, string settingValue, List<ValidationResult> errors,
                bool isValidated, ref bool performedValidation)
            {
                throw new NotImplementedException();
            }
        }


        internal static void ResetCache()
        {
            ItemStore.ClearAll();
        }
    }
}
