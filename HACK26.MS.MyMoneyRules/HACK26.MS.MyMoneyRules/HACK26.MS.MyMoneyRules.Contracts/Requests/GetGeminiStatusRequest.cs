using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class GetGeminiStatusRequest : BaseRequest
    {
    }
}
