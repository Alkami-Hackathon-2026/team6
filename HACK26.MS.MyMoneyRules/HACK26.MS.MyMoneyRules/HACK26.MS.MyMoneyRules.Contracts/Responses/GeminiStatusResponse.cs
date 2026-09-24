using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class GeminiStatusResponse : BaseResponse
    {
        [DataMember]
        public bool Configured { get; set; }

        [DataMember]
        public string Model { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
