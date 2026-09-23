#if NET6_0_OR_GREATER
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    internal class NewRelicOperationBehavior : IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            //do nothing
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            //do nothing
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            var configName = operationDescription?.DeclaringContract?.ConfigurationName;
            string operationName = operationDescription?.TaskMethod?.Name ?? operationDescription.Name;
            if (!string.IsNullOrWhiteSpace(configName))
            {
                operationName = $"{configName}.{operationName}";
            }
            dispatchOperation.Invoker = new NewRelicOperationInvoker(dispatchOperation.Invoker, operationName);
        }

        public void Validate(OperationDescription operationDescription)
        {
            //do nothing
        }
    }
}
#endif
