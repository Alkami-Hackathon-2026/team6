using System;
using System.Collections.Generic;
using NewRelic.Api.Agent;

namespace Alkami.Monitoring.NewRelic
{
    internal interface INewRelicAgent
    {
        IAgent GetAgent();

        void SetApplicationName(string applicationName, string? applicationName2 = null, string? applicationName3 = null);

        void IncrementCounter(string name);

        void NoticeError(Exception exception);

        void NoticeError(Exception exception, IDictionary<string, string>? parameters);

        void NoticeError(string message, IDictionary<string, string>? parameters);

        void RecordMetric(string name, float value);

        void RecordResponseTimeMetric(string name, long millis);

        void RecordCustomEvent(string eventType, IEnumerable<KeyValuePair<string, object>> attributes);

        void IgnoreTransaction();

        void SetTransactionName(string? category, string name);

        void SetUserParameters(string? userName, string? accountName, string? productName);
    }
}
