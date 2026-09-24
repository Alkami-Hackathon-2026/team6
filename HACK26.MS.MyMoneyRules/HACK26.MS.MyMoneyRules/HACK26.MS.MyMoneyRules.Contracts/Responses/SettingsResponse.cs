using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    /// <summary>
    /// Our response object inherits a base object that takes a type argument of the data we want to return
    /// We can add additional properties to this class that may be helpful, it's up to the discretion of the developer
    /// </summary>
    [DataContract(IsReference = true)]
    public class SettingsResponse : BaseResponse<Setting>
    {
    }
}
