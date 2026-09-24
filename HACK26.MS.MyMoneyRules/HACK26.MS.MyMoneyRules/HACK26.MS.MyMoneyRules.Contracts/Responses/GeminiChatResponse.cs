using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    [DataContract(IsReference = true)]
    public class GeminiChatResponse : BaseResponse
    {
        [DataMember]
        public string Model { get; set; }

        [DataMember]
        public string Text { get; set; }
    }
}
