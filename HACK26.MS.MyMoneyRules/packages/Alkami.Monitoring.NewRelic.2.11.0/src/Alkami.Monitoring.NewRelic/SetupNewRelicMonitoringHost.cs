#if NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Alkami.Monitoring.NewRelic
{
    internal class SetupNewRelicMonitoringHost : BackgroundService
    {
        public SetupNewRelicMonitoringHost(NewRelicMonitor monitor)
        {
            Metric.ResetMonitor();
            Metric.MonitorFactory = () => monitor;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
#endif
