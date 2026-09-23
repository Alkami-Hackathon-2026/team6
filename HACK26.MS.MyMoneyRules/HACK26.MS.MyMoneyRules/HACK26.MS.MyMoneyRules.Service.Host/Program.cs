using Alkami.Utilities.Configuration;
using Common.Logging;
using log4net.Config;
using System;
using System.IO;
using Topshelf;

namespace HACK26.MS.MyMoneyRules.Service.Host
{
    internal class Program
    {
        private const string HostName = "HACK26.MS.MyMoneyRules.Service.Host";
        private static readonly ILog Logger = LogManager.GetLogger(HostName);

        private static void Main(string[] args)
        {
            var file = new FileInfo("log4net.config");

            if (file.Exists)
            {
                XmlConfigurator.Configure(file);
                Logger.InfoFormat("Starting {0}", HostName);
            }

            HostFactory.Run(configurator =>
            {
                Alkami.Monitoring.Setup.UseNewRelic();

                // TODO: Use this if your microservice uses EntityFramework.
                //Exceptions.EntityFramework.Setup.UseEntityFrameworkExceptionHandling();

                configurator.UseLog4Net();
                configurator.RunAsLocalSystem();
                configurator.SetDescription("HACK26 MyMoneyRules");
                configurator.SetDisplayName("HACK26 MyMoneyRules");
                configurator.SetServiceName(HostName);

                MicroserviceConfiguration.ConfigureServiceRecovery(configurator);

                configurator.Service<DistributedService>(settings =>
                {
                    settings.ConstructUsing(() => new DistributedService(ServiceImp.StaticName, ServiceImp.StaticProviderName, ServiceImp.StaticProviderType));
                    settings.WhenStarted(withdrawal =>
                    {
                        try
                        {
                            withdrawal.OnStart();
                        }
                        catch (Exception ex)
                        {
                            if (Environment.UserInteractive)
                                Logger.Debug("The service failed to start because of reason: {0}", ex);

                            throw;
                        }
                    });
                    settings.WhenStopped(withdrawal =>
                    {
                        Logger.InfoFormat("Stop called {0}...", HostName);
                        withdrawal.OnStop(TimeSpan.FromSeconds(15));
                    });
                });
            });
        }
    }
}