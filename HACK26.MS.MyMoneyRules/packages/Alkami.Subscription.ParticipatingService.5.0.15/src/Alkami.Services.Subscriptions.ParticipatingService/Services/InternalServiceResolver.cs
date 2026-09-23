using System.Threading.Tasks;
using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.Resolver;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    internal class InternalServiceResolver : IInternalServiceResolver
    {
        public bool EstablishedConnectionAtLeastOnce => ServiceResolver.EstablishedConnectionAtLeastOnce;

        public Task ForceRefresh()
        {
            return ServiceResolver.ForceRefresh();
        }

        public Task Register(ServiceDefinition serviceDefinition)
        {
            return ServiceResolver.RegisterAsync(serviceDefinition);
        }

        public Task Unregister(ServiceDefinition serviceDefinition)
        {
            return ServiceResolver.UnRegisterAsync(serviceDefinition);
        }
    }
}
