#if NETFRAMEWORK
using Alkami.Contracts;

namespace Alkami.Services.Subscriptions.ParticipatingService
{

    internal class BaseErrorHandlerResult
    {
        public BaseResponse ReturnValue { get; set; }
        public object[] Outputs { get; set; }

        public object GetValue(object[] inputs, out object[] outputs)
        {
            outputs = this.Outputs;
            return this.ReturnValue;
        }
    }
}
#endif
