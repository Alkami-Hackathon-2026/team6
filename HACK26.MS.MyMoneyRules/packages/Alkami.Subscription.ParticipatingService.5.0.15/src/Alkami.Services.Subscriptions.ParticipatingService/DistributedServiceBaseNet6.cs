#if NET6_0_OR_GREATER

using CoreWCF;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.PerCall)]
    public abstract partial class DistributedServiceBase<T>
    {
        protected DistributedServiceBase() { }
    }
}
#endif