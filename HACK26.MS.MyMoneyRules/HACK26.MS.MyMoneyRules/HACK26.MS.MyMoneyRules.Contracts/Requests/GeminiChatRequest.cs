using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class GeminiChatRequest : BaseRequest
    {
        [DataMember]
        public string Prompt { get; set; }

        [DataMember]
        public string SystemInstruction { get; set; }
    }
}
