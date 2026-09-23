using Alkami.Contracts;
using Alkami.MicroServices.Settings.ProviderBasedClient;
using HACK26.MS.MyMoneyRules.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Client
{
    /// <inheritdoc />
    public class MyMoneyRulesServiceClient : ProviderBasedClient<IMyMoneyRulesServiceContract>, IMyMoneyRulesServiceContract
    {
        /// <summary>
        /// The unique provider type designator for this service
        /// </summary>
        private const string ProviderType = "HACK26";

        /// <summary>
        ///  ProviderBased constructor
        /// </summary>
        public MyMoneyRulesServiceClient() : base(ProviderType)
        { }

        /// <summary>
        /// ProviderBased constructor override
        /// </summary>
        /// <param name="providerId"></param>
        public MyMoneyRulesServiceClient(long providerId) : base(ProviderType, providerId)
        { }

        /// <inheritdoc />
        public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetSettingsAsync(inner), request);
        }

        /// <inheritdoc />
        public Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetDataAsync(inner), request);
        }

        /// <inheritdoc />
        public Task<FdicComplianceConfigsResponse> GetFDICConfigurationAsync(GetSomethingRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetFDICConfigurationAsync(inner), request);
        }
    }
}