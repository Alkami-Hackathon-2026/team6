#if NET6_0_OR_GREATER
using System;
using Alkami.Monitoring;
using Alkami.Monitoring.NewRelic;
using Alkami.Utilities.Kubernetes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alkami.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> Extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds NewRelic Monitoring to the Application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add services.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> instance to get configuration.</param>
        /// <returns>A reference to the provided <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddNewRelicMonitor(this IServiceCollection services, IConfiguration configuration)
        {
            return AddNewRelicMonitor(services, configuration, new NewRelicAgent(), new SettingsWrapperImp(), null);
        }

        /// <summary>
        /// Adds NewRelic Monitoring to the Application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add services.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> instance to get configuration.</param>
        /// <param name="configureOptions">The optional action to further customize the <see cref="NewRelicMonitorOptions"/>.</param>
        /// <returns>A reference to the provided <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddNewRelicMonitor(this IServiceCollection services, IConfiguration configuration, Action<NewRelicMonitorOptions>? configureOptions)
        {
            return AddNewRelicMonitor(services, configuration, new NewRelicAgent(), new SettingsWrapperImp(), configureOptions);
        }

        internal static IServiceCollection AddNewRelicMonitor(IServiceCollection services, IConfiguration configuration, INewRelicAgent newRelicAgent, ISettingsWrapper configSettings, Action<NewRelicMonitorOptions>? configureOptions)
        {
            var options = new NewRelicMonitorOptions();

            if (Setup.IsRunningInKubernetes())
            {
                options.ApplicationName = configuration.GetValue<string?>(Setup.NewRelicAppName_EnvironmentVariable);
                options.Prefix = null;

                var areaName = GetApplicationName(configuration, configSettings);
                if (!string.IsNullOrWhiteSpace(areaName))
                {
                    options.AreaName = areaName;
                }
            }
            else
            {
                options.ApplicationName = GetApplicationName(configuration, configSettings);
                options.Prefix = GetPrefix(configuration, configSettings);

                if (!string.IsNullOrWhiteSpace(options.ApplicationName))
                {
                    options.AreaName = options.ApplicationName;
                }
            }

            configureOptions?.Invoke(options);

            var appName = options.GeneratedApplicationName();
            if (!string.IsNullOrWhiteSpace(appName))
            {
                newRelicAgent.SetApplicationName(appName);
            }

            Metric.AreaName = options.AreaName;
            var factoryMonitoring = Metric.MonitorFactory == null ? null : Metric.MonitorFactory();

            if (factoryMonitoring == null || factoryMonitoring is NoOpMonitor)
            {
                services.AddHostedService<SetupNewRelicMonitoringHost>();
                services.TryAddSingleton<NewRelicMonitor>();
            }
            else
            {
                services.TryAddSingleton((sp) =>
                {
                    var factoryMonitoring = Metric.MonitorFactory == null ? null : Metric.MonitorFactory();
                    return factoryMonitoring as NewRelicMonitor ?? ActivatorUtilities.CreateInstance<NewRelicMonitor>(sp);
                });
            }

            services.TryAddTransient<IMonitor>(x => x.GetRequiredService<NewRelicMonitor>());
            services.TryAddTransient<IMonitorProperties>(x => x.GetRequiredService<NewRelicMonitor>());

            return services;
        }

        private static string GetApplicationName(IConfiguration configuration, ISettingsWrapper configSettings)
        {
            string? result = configuration.GetValue<string?>(Setup.NewRelicAppName);

            if (string.IsNullOrWhiteSpace(result))
            {
                result = configSettings.GetSetting(Setup.NewRelicAppName);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = configuration.GetValue<string?>(Setup.PackageName);
            }

            return result ?? string.Empty;
        }

        private static string GetPrefix(IConfiguration configuration, ISettingsWrapper configSettings)
        {
            string? result = configuration.GetValue<string?>(Setup.NewRelicPrefix);

            if (string.IsNullOrWhiteSpace(result))
            {
                result = configSettings.GetSetting(Setup.NewRelicPrefix);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = configuration.GetValue<string?>(Setup.EnvironmentName_EnvironmentVariable);
            }

            return result ?? string.Empty;
        }

    }
}

#endif
