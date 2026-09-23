using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Alkami.Monitoring
{
    /// <summary>
    /// Default Monitor that is no op
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NoOpMonitor : IMonitor, IMonitorProperties
    {

        private string areaName = "Unknown";

        /// <inheritdoc />
        public string AreaName
        {
            get { return areaName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Remove any separator characters.
                    areaName = value.Replace(Metric.Separator, '-');
                }
                else
                {
                    areaName = value;
                }
            }
        }

        /// <inheritdoc />
        public void AddCustomProperty(string name, string value)
        {
            //do nothing
        }

        /// <inheritdoc />
        public string CreateName(params string[] orderedSegments)
        {
            return string.Join(Metric.Separator.ToString(), new[] { "Custom", AreaName }.Concat(orderedSegments));
        }

        /// <inheritdoc />
        public void IncrementCounter(string name)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void IncrementCounter(params string[] orderedSegments)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void NoticeError(Exception exception)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void NoticeError(Exception exception, IDictionary<string, string> parameters)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void NoticeError(string message, IDictionary<string, string> parameters)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RecordCustomEvent(string name, MonitorEvent payload)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RecordMetric(string name, float value)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RecordMetric(float value, params string[] orderedSegments)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RecordResponseTimeMetric(string name, TimeSpan duration)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RecordResponseTimeMetric(TimeSpan duration, params string[] orderedSegments)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void SetTransactionName(string category, string name)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void SetUserProperties(string userValue, string accountValue, string productValue)
        {
            //do nothing
        }

    }
}
