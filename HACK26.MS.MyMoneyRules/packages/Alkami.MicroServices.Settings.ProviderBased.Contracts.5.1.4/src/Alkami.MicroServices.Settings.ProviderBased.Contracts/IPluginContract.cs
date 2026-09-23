using System.ServiceModel;
using System.Threading.Tasks;
using Alkami.Security;
using Alkami.MicroServices.Settings.ProviderBased.Contracts.Requests;
using Alkami.MicroServices.Settings.ProviderBased.Contracts.Responses;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts
{
    [ServiceContract]
    public interface IPluginContract
    {
        [OperationContract]
        [RequiresPermissions(Permission.BankEntity)]
        Task<SettingDescriptorsResponse> GetSettingDescriptorsAsync(ProviderSettingsRequest request);

        [OperationContract]
        [RequiresPermissions(Permission.BankEntity)]
        Task<ProviderSettingsResponse> GetDefaultSettingsAsync(ProviderSettingsRequest request);

        [OperationContract]
        [RequiresPermissions(Permission.BankEntity)]
        Task<ValidateChangedSettingsResponse> ValidateChangedSettingsAsync(ValidateChangedSettingsRequest request);
    }
}
