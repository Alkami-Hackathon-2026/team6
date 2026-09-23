#if NET6_0_OR_GREATER
using Alkami.Extensions.RpcHost.Health;
using Alkami.Utilities.Kubernetes;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    /// <summary>
    /// The listener port configuration
    /// </summary>
    internal static class ListenerPortConfiguration
    {
        /// <summary>
        /// The port the application listens on
        /// </summary>
        public static readonly int AppPort = ServiceUrlSettings.GetAppPort();

        /// <summary>
        /// The port the health checks listen on
        /// </summary>
        public static readonly int HealthPort = HealthListenerPortConfiguration.GetHealthPort();
    }
}
#endif
