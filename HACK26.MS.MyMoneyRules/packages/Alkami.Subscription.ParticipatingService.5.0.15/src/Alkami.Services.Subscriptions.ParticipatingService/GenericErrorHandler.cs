#if NET5_0_OR_GREATER
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
#endif
#if NETFRAMEWORK
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
#endif

namespace Alkami.Services.Subscriptions.ParticipatingService
{
	public class GenericErrorHandler : IOperationBehavior
	{
		public void Validate(OperationDescription operationDescription)
		{
		}

		public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{
			dispatchOperation.Invoker = new BaseErrorHandler(dispatchOperation.Invoker, operationDescription, operationDescription.Name);
		}

		public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
		{
		}

		public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
		{
		}
	}
}