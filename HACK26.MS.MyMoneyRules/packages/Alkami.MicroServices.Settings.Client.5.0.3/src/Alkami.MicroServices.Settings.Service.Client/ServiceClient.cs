using Alkami.MicroServices.Settings.Contracts;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Contracts.Responses;
using Alkami.Services.Subscriptions.ParticipatingClient;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Settings.Service.Client
{
    /// <summary>
    /// Client for calling into the Settings Microservice
    /// </summary>
    public class ServiceClient : SelfResolvingClient<ISettingsServiceContract>, ISettingsServiceContract
    {
        /// <inheritdoc />
        public Task<ItemResponse> AddOrUpdateItemsAsync(AddOrUpdateItemRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateItemsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<UserWidgetSettingResponse> AddOrUpdateUserWidgetSettingAsync(AddOrUpdateUserWidgetSettingRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateUserWidgetSettingAsync(r), request);
        }

        /// <inheritdoc />
        public Task<WidgetsResponse> AddOrUpdateWidgetsAsync(AddOrUpdateWidgetRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateWidgetsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<WidgetSettingResponse> AddOrUpdateWidgetSettingAsync(AddOrUpdateWidgetSettingRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateWidgetSettingAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ProviderTypeResponse> AddOrUpdateProviderTypeAsync(AddOrUpdateProviderTypeRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateProviderTypeAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ProviderResponse> AddOrUpdateProviderAsync(AddOrUpdateProviderRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateProviderAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ItemResponse> GetItemsAsync(GetItemRequest request)
        {
            return ProxyCall((c, r) => c.GetItemsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ProviderTypeResponse> GetProviderTypeAsync(GetProviderTypeRequest request)
        {
            return ProxyCall((c, r) => c.GetProviderTypeAsync(r), request);
        }

        /// <inheritdoc />
        public Task<UserWidgetSettingResponse> GetUserWidgetSettingsAsync(GetUserWidgetSettingRequest request)
        {
            return ProxyCall((c, r) => c.GetUserWidgetSettingsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<WidgetsResponse> GetWidgetsAsync(GetWidgetsRequest request)
        {
            return ProxyCall((c, r) => c.GetWidgetsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<WidgetSettingResponse> GetWidgetSettingsAsync(GetWidgetSettingRequest request)
        {
            return ProxyCall((c, r) => c.GetWidgetSettingsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<BankResponse> GetBanksAsync(GetBanksRequest request)
        {
            return ProxyCall((c, r) => c.GetBanksAsync(r), request);
        }

        /// <inheritdoc />
        public Task<LocalizableResourceResponse> GetOrAddLocalizableResourcesAsync(GetOrAddLocalizableResourceRequest request)
        {
            return ProxyCall((c, r) => c.GetOrAddLocalizableResourcesAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ProviderSettingResponse> GetProviderSettingsAsync(GetProviderSettingRequest request)
        {
            return ProxyCall((c, r) => c.GetProviderSettingsAsync(r), request);
        }

        /// <inheritdoc />
        public Task<ProviderSettingResponse> AddOrUpdateProviderSettingsAsync(AddOrUpdateProviderSettingRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateProviderSettingsAsync(r), request);
        }
    }
}
