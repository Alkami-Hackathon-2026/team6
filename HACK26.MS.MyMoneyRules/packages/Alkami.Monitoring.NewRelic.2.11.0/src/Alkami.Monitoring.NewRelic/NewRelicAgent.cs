using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NewRelicApi = NewRelic.Api.Agent.NewRelic;

namespace Alkami.Monitoring.NewRelic
{
    [ExcludeFromCodeCoverage]
    internal class NewRelicAgent : INewRelicAgent
    {
        public IAgent GetAgent()
        {
            return NewRelicApi.GetAgent();
        }

        public void IgnoreTransaction()
        {
            NewRelicApi.IgnoreTransaction();
        }

        public void IncrementCounter(string name)
        {
            NewRelicApi.IncrementCounter(name);
        }

        public void NoticeError(Exception exception)
        {
            NewRelicApi.NoticeError(exception);
        }

        public void NoticeError(Exception exception, IDictionary<string, string>? parameters)
        {
            NewRelicApi.NoticeError(exception, parameters);
        }

        public void NoticeError(string message, IDictionary<string, string>? parameters)
        {
            NewRelicApi.NoticeError(message, parameters);
        }

        public void RecordCustomEvent(string eventType, IEnumerable<KeyValuePair<string, object>> attributes)
        {
            NewRelicApi.RecordCustomEvent(eventType, attributes);
        }

        public void RecordMetric(string name, float value)
        {
            NewRelicApi.RecordMetric(name, value);
        }

        public void RecordResponseTimeMetric(string name, long millis)
        {
            NewRelicApi.RecordResponseTimeMetric(name, millis);
        }

        public void SetApplicationName(string applicationName, string? applicationName2 = null, string? applicationName3 = null)
        {
            NewRelicApi.SetApplicationName(applicationName, applicationName2, applicationName3);
        }

        public void SetTransactionName(string? category, string name)
        {
            NewRelicApi.SetTransactionName(category, name);
        }

        public void SetUserParameters(string? userName, string? accountName, string? productName)
        {
            NewRelicApi.SetUserParameters(userName, accountName, productName);
        }
    }
}
