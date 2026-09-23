using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Monitoring;
using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#if NET5_0_OR_GREATER
using CoreWCF.Dispatcher;
#endif
#if NETFRAMEWORK
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
#endif

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestResponseInspector : IParameterInspector
    {
        private static readonly ILog Logger = LogManager.GetLogger<RequestResponseInspector>();

        static RequestResponseInspector()
        {
            EntityValidator.AddValidator(new BaseRequestValidator());
        }

        private ConcurrentQueue<TimeSpan> _callDurations = new ConcurrentQueue<TimeSpan>();

        private ConcurrentDictionary<string, int> _errorsByCode = new ConcurrentDictionary<string, int>();

        private int _msgCount;

        private ConcurrentDictionary<Guid, int> _tenantCounts = new ConcurrentDictionary<Guid, int>();

        private int _highWaterMark;

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            var output = returnValue as BaseResponse;
            if (output == null)
            {
                return;
            }
            var parts = correlationState as CorrelationState;

            if (parts != null)
                output.CorrelateWithRequest(parts.Request, parts.Stopwatch.Elapsed);

            output.Server = Environment.MachineName;
            output.ProcessId = Process.GetCurrentProcess().Id;

            _callDurations.Enqueue(parts.Stopwatch.Elapsed);
            Interlocked.Increment(ref _msgCount);

            Metric.RecordResponseTimeMetric(parts.Stopwatch.Elapsed, operationName + "ResponseTime");

            if (output.ValidationResults != null)
            {
                // Only add validation results from the service (i.e. the Origin is null).
                foreach (var vr in output.ValidationResults.Where(vr => string.IsNullOrWhiteSpace(vr.Origin)))
                {
                    var errorCodeKey = vr.ErrorCode.ToString();
                    int count;

                    _errorsByCode.TryGetValue(errorCodeKey, out count);
                    _errorsByCode[errorCodeKey] = ++count;
                }
            }

            if (output.ValidationResults != null)
            {
                var originName = string.Concat(Metric.AreaName, " ", operationName);

                foreach (var vr in output.ValidationResults)
                    if (string.IsNullOrWhiteSpace(vr.Origin))
                        vr.Origin = originName;
            }

            if (Logger.IsTraceEnabled)
                Logger.TraceFormat("OperationName: {0}, Response: {1}", operationName, output);
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            Metric.SetTransactionName("WCF", operationName);

            var message = inputs.FirstOrDefault() as BaseRequest;

            if (message != null)
            {
                InjectLoggingProperties(message);

                var bankIdentifier = message.BankIdentifier.GetValueOrDefault();
                int count;

                _tenantCounts.TryGetValue(bankIdentifier, out count);
                _tenantCounts[bankIdentifier] = ++count;

                if (Logger.IsTraceEnabled)
                    Logger.TraceFormat("OperationName: {0}, Request: {1}", operationName, message);

                return new CorrelationState { Request = message, Stopwatch = Stopwatch.StartNew() };
            }
            return null;
        }

        public Timings GetAndResetAverageDuration()
        {
            var copy = Interlocked.Exchange(ref _callDurations, new ConcurrentQueue<TimeSpan>());
            if (!copy.Any())
                return new Timings(new List<TimeSpan>());
            return new Timings(copy.ToList());
        }

        public ConcurrentDictionary<string, int> GetAndResetErrorsByCode()
        {
            return Interlocked.Exchange(ref _errorsByCode, new ConcurrentDictionary<string, int>());
        }

        public int GetAndResetMessageCount()
        {
            Interlocked.Exchange(ref _highWaterMark, Math.Max(_highWaterMark, _msgCount));
            return Interlocked.Exchange(ref _msgCount, 0);

        }

        public int HighWaterMark
        {
            get { return _highWaterMark; }
        }

        public ConcurrentDictionary<Guid, int> GetAndResetTenantCounts()
        {
            return Interlocked.Exchange(ref _tenantCounts, new ConcurrentDictionary<Guid, int>());
        }

        private static void InjectLoggingProperties(BaseRequest request)
        {
#if NETFRAMEWORK
            log4net.LogicalThreadContext.Properties["BankName"] = request.BankUri;
            log4net.LogicalThreadContext.Properties["BankIdentifier"] = request.BankIdentifier;
            log4net.LogicalThreadContext.Properties["UserIdentifier"] = request.UserIdentifier;
            log4net.LogicalThreadContext.Properties["UserSessionId"] = request.MessageIdentifier;
            log4net.LogicalThreadContext.Properties["CorrelationID"] = request.CorrelationId;
            log4net.LogicalThreadContext.Properties["MessageIdentifier"] = request.MessageIdentifier;
#endif
        }

        internal class CorrelationState
        {
            public BaseRequest Request { get; set; }

            public Stopwatch Stopwatch { get; set; }
        }

#if NETFRAMEWORK

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            throw new NotImplementedException();
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
