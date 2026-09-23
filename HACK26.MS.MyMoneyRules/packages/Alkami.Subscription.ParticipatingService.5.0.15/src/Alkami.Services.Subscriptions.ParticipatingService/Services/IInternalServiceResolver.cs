using System.Threading.Tasks;
using Alkami.Services.Subscriptions.Data;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    internal interface IInternalServiceResolver
    {
        bool EstablishedConnectionAtLeastOnce { get; }

        Task ForceRefresh();

        Task Register(ServiceDefinition serviceDefinition);

        Task Unregister(ServiceDefinition serviceDefinition);
    }
}
