using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Alkami.Data.Validations;
using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.ParticipatingService;
using Alkami.MicroServices.Settings.ProviderBased.Contracts;
using Alkami.MicroServices.Settings.ProviderBased.Contracts.Requests;
using Alkami.MicroServices.Settings.ProviderBased.Contracts.Responses;
using Alkami.TrackableObjects.Plugins;
using System.Linq.Expressions;

namespace Alkami.MicroServices.Settings.ProviderBasedService
{
    /// <summary>
    /// This is designed for provider based services that implement the same interface. There will be several of these
    /// services sharing the same interface, but utilizing different providers
    /// </summary>
    /// <typeparam name="T">The Provider Interface</typeparam>
    public abstract class ProviderBasedService<T, TPlugin> : DistributedServiceBase<T>, IPluginContract where TPlugin : Plugin
    {
        private TPlugin _serviceContract;
        private string _providerType;
        private string _providerName;

        protected ProviderBasedService(string friendlyName, string providerName, string providerType) : base(friendlyName, typeof(T).FullName)
        {
            _providerType = providerType;
            _providerName = providerName;

            _myServiceDefinition.ProviderConfiguration =
                new ProviderConfiguration()
                {
                    Name = providerName,
                    ProviderType = providerType
                };

            var pluginQualifiedName = "Plugin";
            var pluginEndpointAddress = $"{EndpointAddress}/{pluginQualifiedName}";

            var endpoint = Host.AddServiceEndpoint
            (
                typeof(IPluginContract).FullName,
                SubscriptionBindings.Binding(),
                pluginEndpointAddress
            );

            foreach (var operation in endpoint.Contract.Operations)
            {
                operation.Behaviors.Add(new GenericErrorHandler());
                var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (serializer != null)
                {
                    serializer.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Getter contains default implementation of constructor call to TPlugin. Expects a constructor with two strings e.g. ctor(providerType, providerName)
        /// If you have a custom constructor that is necessary, use the setter with your instance to prevent the default functionality
        /// </summary>
        protected virtual TPlugin ServiceContract
        {
            get
            {
                if (_serviceContract == null)
                {
                    //string providerType, string providerName OR just providerType
                    var constructor = typeof(TPlugin)
                        .GetConstructors()
                        .Where(x => x.GetParameters().Count() == 2 && x.GetParameters().All(p => p.ParameterType == typeof(string)))
                        .FirstOrDefault();

                    if (constructor != null)
                    {
                        _serviceContract = Expression.Lambda<Func<TPlugin>>(Expression.New(constructor, Expression.Constant(_providerType), Expression.Constant(_providerName))).Compile().Invoke();
                    }
                }

                return _serviceContract;
            }
            set
            {
                _serviceContract = value;
            }
        }

        public async Task<SettingDescriptorsResponse> GetSettingDescriptorsAsync(ProviderSettingsRequest request)
        {
            ValidateServiceContract();

            var response = new SettingDescriptorsResponse()
            {
                SettingDescriptors = ServiceContract.SettingDescriptors()
            };

            return response;
        }

        public async Task<ProviderSettingsResponse> GetDefaultSettingsAsync(ProviderSettingsRequest request)
        {
            ValidateServiceContract();

            var response = new ProviderSettingsResponse()
            {
                ProviderSettings = ServiceContract.DefaultSettings()
            };
            return response;
        }

        public async Task<ValidateChangedSettingsResponse> ValidateChangedSettingsAsync(
            ValidateChangedSettingsRequest request)
        {
            ValidateServiceContract();

            List<ValidationResult> result;
            var isValid = ServiceContract.ValidateChangedSettings(request.SettingsToValidate, out result);
            var response = new ValidateChangedSettingsResponse()
            {
                HasError = !isValid,
                ValidationResults = result
            };
            return response;
        }

        private void ValidateServiceContract()
        {
            if (ServiceContract == null)
            {
                throw new NullReferenceException("Service Contract must return a valid instance.");
            }
        }
    }
}