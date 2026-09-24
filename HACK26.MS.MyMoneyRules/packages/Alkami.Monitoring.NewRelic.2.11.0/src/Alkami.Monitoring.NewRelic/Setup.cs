using Alkami.Monitoring.NewRelic;
using System;
using NewRelicApi = NewRelic.Api.Agent.NewRelic;

namespace Alkami.Monitoring
{
    /// <summary>
    /// Setup class for the <see cref="Metric"/> class.
    /// </summary>
    public static class Setup
    {
        internal const string NewRelicAppName = "NewRelic.AppName";
        internal const string NewRelicAppName_EnvironmentVariable = "NEW_RELIC_APP_NAME";
        internal const string NewRelicPrefix = "NewRelicPrefix";
        internal const string PackageName = "PackageName";
        internal const string EnvironmentName = "Environment.Name";
        internal const string EnvironmentName_EnvironmentVariable = "ALKAMI_ENVIRONMENT_FULLNAME";

        /// <summary>
        /// Settings service wrapper that can fetch setting values
        /// </summary>
        public static ISettingsWrapper ConfigSettings = new SettingsWrapperImp();

#if NET6_0_OR_GREATER
        internal static Func<bool> _DefaultIsRunningInKubernetes => () => Utilities.Kubernetes.ServiceUrlSettings.IsRunningInKubernetes();
        internal static Func<bool> IsRunningInKubernetes = _DefaultIsRunningInKubernetes;
#endif

        /// <summary>
        /// Setup Monitoring to use <see cref="NewRelicMonitor"/>.
        /// </summary>
        public static void UseNewRelic()
        {
            Metric.MonitorFactory = () => new NewRelicMonitor();

            var appName = GetNewRelicAppName();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                NewRelicApi.SetApplicationName(appName);
            }
        }

        /// <summary>
        /// Finds the app name for New Relic.
        /// </summary>
        /// <returns> It returns the desired app name to be set for New Relic Transaction. </returns>
        public static string GetNewRelicAppName()
        {
#if NET6_0_OR_GREATER
            if (IsRunningInKubernetes())
            {
                return ConfigSettings.GetEnvironmentVariable(NewRelicAppName_EnvironmentVariable) ?? string.Empty;
            }
#endif

            var newRelicAppName = "";

            var appName = ConfigSettings.GetSetting(NewRelicAppName);
#if NET6_0_OR_GREATER
            var envName = ConfigSettings.GetEnvironmentVariable(EnvironmentName_EnvironmentVariable);
#else
            var envName = ConfigSettings.GetSetting(EnvironmentName);
#endif
            var prefixName = ConfigSettings.GetSetting(NewRelicPrefix);

            var additionalName = "";
            // If NewRelicPrefix setting doesn't exist, then app name contains Environment.Name
            if (!string.IsNullOrEmpty(prefixName))
            {
                additionalName = prefixName;
            }
            else if (!string.IsNullOrEmpty(envName))
            {
                additionalName = envName;
            }

            // if appName is not available from config, try to get it from path
            if (string.IsNullOrEmpty(appName))
            {
                var path = Environment.GetCommandLineArgs()[0];
                var index = path.LastIndexOf('\\');
                // Get only the name of the file and not the whole location
                appName = index != -1 ? path.Substring(index + 1) : path;

                index = appName.LastIndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                // Removing the exe file extension
                if (index == appName.Length - 4)
                {
                    appName = appName.Substring(0, index);
                }
            }

            // if appName and additionalName are available, return the addition of them
            // if appName already contains the additionalName, return just the appName
            if (!string.IsNullOrEmpty(appName) && !string.IsNullOrEmpty(additionalName))
            {
                if (!appName.Contains(additionalName, StringComparison.OrdinalIgnoreCase))
                {
                    newRelicAppName = additionalName + " " + appName;
                }
                else
                {
                    newRelicAppName = appName;
                }
            }
            // if additionalName is not available, return appName
            else if (!string.IsNullOrEmpty(appName))
            {
                newRelicAppName = appName;
            }

            return newRelicAppName;
        }
    }
}
