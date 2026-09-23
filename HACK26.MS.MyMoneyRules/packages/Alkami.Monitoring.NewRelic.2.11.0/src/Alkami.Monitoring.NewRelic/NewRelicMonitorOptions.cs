#if NET6_0_OR_GREATER
using System;

namespace Alkami.Monitoring.NewRelic
{
    /// <summary>
    /// Options for the <see cref="NewRelicMonitor"/>
    /// </summary>
    public class NewRelicMonitorOptions
    {
        internal static readonly char Separator = '/';
        private string _areaName = "Unknown";

        /// <summary>
        /// The Application Name to set the NewRelic monitor to.
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// The Prefix to include before the <see cref="ApplicationName"/> of the NewRelic monitor. When running in k8s this value is ignored.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// The name of the area for metrics collection. Any character of '/' will be changed to '-', during area name creation
        /// </summary>
        public string AreaName
        {
            get => _areaName;
            set => _areaName = value.Replace(Separator, '-');
        }

        /// <summary>
        /// Generates the name that will be set in NewRelic based on the <see cref="ApplicationName"/> and <see cref="Prefix"/>
        /// </summary>
        /// <returns>The generated application name</returns>
        public string? GeneratedApplicationName()
        {
            if (string.IsNullOrWhiteSpace(ApplicationName))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(Prefix))
            {
                return ApplicationName;
            }

            if (ApplicationName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationName;
            }

            return $"{Prefix} {ApplicationName}";

        }
    }
}

#endif
