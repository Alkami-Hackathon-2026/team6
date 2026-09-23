using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
#if NET6_0_OR_GREATER
using Microsoft.Extensions.Logging;
#endif

namespace Alkami.Monitoring.NewRelic
{
    /// <summary>
    /// The New Relic Monitor is a concrete implementation of the IMonitor interface for logging performance metrics to New Relic.
    /// </summary> 
    public class NewRelicMonitor : IMonitor, IMonitorProperties
    {
        private readonly ILog Logger = LogManager.GetLogger<NewRelicMonitor>();

#if NET6_0_OR_GREATER
        private readonly ILogger<NewRelicMonitor>? LoggerNet6;
#endif

        private INewRelicAgent NewRelicApi { get; }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Default Constructor
        /// </summary>
        public NewRelicMonitor() : this(null, new NewRelicAgent())
        {

        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NewRelicMonitor(ILogger<NewRelicMonitor> logger) : this(logger, new NewRelicAgent())
        {

        }

        internal NewRelicMonitor(ILogger<NewRelicMonitor>? logger, INewRelicAgent newRelicApi)
        {
            LoggerNet6 = logger;
            NewRelicApi = newRelicApi;
        }
#else

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NewRelicMonitor() : this(new NewRelicAgent())
        {

        }

        internal NewRelicMonitor(INewRelicAgent newRelicApi)
        {
            NewRelicApi = newRelicApi;
        }
#endif

        /// <inheritdoc />
        public string AreaName { get; set; } = "Unknown";

        /// <inheritdoc />
        public string CreateName(params string[] orderedSegments)
        {
            return string.Join(Metric.Separator.ToString(), new[] { "Custom", AreaName }.Concat(orderedSegments));
        }

        /// <inheritdoc/>
        public virtual void IncrementCounter(string name)
        {
            try
            {
                NewRelicApi.IncrementCounter(name);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc/>
        public void IncrementCounter(params string[] orderedSegments)
        {
            IncrementCounter(CreateName(orderedSegments));
        }

        /// <inheritdoc/>
        public void NoticeError(Exception exception)
        {
            try
            {
                NewRelicApi.NoticeError(exception);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc/>
        public void NoticeError(Exception exception, IDictionary<string, string> parameters)
        {
            try
            {
                NewRelicApi.NoticeError(exception, parameters);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc/>
        public void NoticeError(string message, IDictionary<string, string> parameters)
        {
            try
            {
                NewRelicApi.NoticeError(message, parameters);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc/>
        public virtual void RecordMetric(string name, float value)
        {
            try
            {
                NewRelicApi.RecordMetric(name, value);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc />
        public void RecordMetric(float value, params string[] orderedSegments)
        {
            RecordMetric(CreateName(orderedSegments), value);
        }

        /// <inheritdoc/>
        public virtual void RecordResponseTimeMetric(string name, TimeSpan duration)
        {
            try
            {
                NewRelicApi.RecordResponseTimeMetric(name, (long)duration.TotalMilliseconds);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc />
        public void RecordResponseTimeMetric(TimeSpan duration, params string[] orderedSegments)
        {
            RecordResponseTimeMetric(CreateName(orderedSegments), duration);
        }

        /// <inheritdoc/>
        public virtual void RecordCustomEvent(string name, MonitorEvent payload)
        {
            try
            {
                NewRelicApi.RecordCustomEvent(name, payload);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        /// <inheritdoc />
        public void IgnoreTransaction()
        {
            NewRelicApi.IgnoreTransaction();
        }

        /// <inheritdoc/>
        public void AddCustomProperty(string name, string value)
        {
            NewRelicApi.GetAgent()?.CurrentTransaction?.AddCustomAttribute(name, value);
        }

        /// <inheritdoc/>
        public void SetTransactionName(string category, string name)
        {
            NewRelicApi.SetTransactionName(category, name);
        }

        /// <inheritdoc/>
        public void SetUserProperties(string userValue, string accountValue, string productValue)
        {
            NewRelicApi.SetUserParameters(userValue, accountValue, productValue);
        }

        private void LogError(Exception ex)
        {
#if NET6_0_OR_GREATER
            if (LoggerNet6 != null)
            {
                LoggerNet6.LogError(ex, ex.Message);
                return;
            }
#endif
            Logger.Error(ex);
        }

    }
}
