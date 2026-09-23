#if NET6_0_OR_GREATER
using System.Threading.Tasks;
using Alkami.Monitoring;
using CoreWCF.Dispatcher;
using NewRelic.Api.Agent;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    internal class NewRelicOperationInvoker : IOperationInvoker
    {
        private readonly IOperationInvoker _innerInvoker;
        private readonly string _operationName;

        public NewRelicOperationInvoker(IOperationInvoker innerInvoker, string operationName)
        {
            _innerInvoker = innerInvoker;
            _operationName = operationName;
        }

        public object[] AllocateInputs()
        {
            return _innerInvoker.AllocateInputs();
        }

        [Transaction(Web = true)]
        public ValueTask<(object returnValue, object[] outputs)> InvokeAsync(object instance, object[] inputs)
        {
            Metric.SetTransactionName("WCF", _operationName);
            return this._innerInvoker.InvokeAsync(instance, inputs);
        }
    }
}
#endif
