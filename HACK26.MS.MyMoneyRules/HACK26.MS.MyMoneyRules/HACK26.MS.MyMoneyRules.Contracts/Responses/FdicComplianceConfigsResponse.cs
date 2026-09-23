using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class FdicComplianceConfigsResponse : BaseResponse<FdicComplianceConfigs>
    {
    }
}
