#if NET5_0_OR_GREATER
using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Exceptions;
using Alkami.Monitoring;
using Alkami.Security;
using CoreWCF.Dispatcher;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public partial class BaseErrorHandler : IOperationInvoker
    {
        public async ValueTask<(object returnValue, object[] outputs)> InvokeAsync(object instance, object[] inputs)
        {
            IDisposable logContext = null;
            try
            {
                var validationResult = this.PerformValidation(inputs);
                var baseRequest = inputs[0] as BaseRequest;

                logContext = AddBaseRequestPropertiesToLogContext(baseRequest);

                if (validationResult.HasError)
                {
                    var outputs = new object[] { validationResult };

                    return (validationResult, outputs);
                }
                else
                {
                    var (response, outputs) = await _innerInvoker.InvokeAsync(instance, inputs);

                    var invokeResponse = (BaseResponse)response;

                    // Add any warnings produced during validation to the service's response.
                    invokeResponse.AddValidationResultsFrom(validationResult);

                    if (invokeResponse.HasErrorExtended())
                        MetricLogger.HandleResponseError(invokeResponse, _operationName);

                    outputs = new object[] { invokeResponse };

                    return (invokeResponse, outputs);
                }
            }
            catch (Exception e)
            {
                Metric.NoticeError(e);
                Logger.ErrorFormat("Unhandled error during Invoke:", e);

                var alk = AlkamiException.Parse(e);
                var result = _faultedOutput();

                result.HasError = true;
                result.ValidationResults = new List<ValidationResult>()
                {
                    new ValidationResult()
                    {
                        ErrorCode = alk.ErrorCode,
                        SubCode = alk.SubCode,
                        Severity = Severity.Fatal,
                        Message = alk.Message
                    }
                };

                result.SystemMessage = alk.Message;

                MetricLogger.HandleResponseError(result, _operationName);

                var outputs = new object[] { result };

                return (result, outputs);
            }
            finally
            {
                logContext?.Dispose();
            }
        }

        public virtual IDisposable AddBaseRequestPropertiesToLogContext(BaseRequest baseRequest)
        {
            if (baseRequest == null)
                return null;

            LogContext.PushProperty(nameof(BaseRequest.CorrelationId), baseRequest.CorrelationId);
            LogContext.PushProperty(nameof(BaseRequest.MessageIdentifier), baseRequest.MessageIdentifier.ToString());

            PushProp(baseRequest, baseRequest.UserIdentifier?.ToString(), nameof(baseRequest.UserIdentifier), AlkamiClaimTypes.UserIdentifier);
            PushProp(baseRequest, baseRequest.BankIdentifier?.ToString(), nameof(baseRequest.BankIdentifier), AlkamiClaimTypes.BankIdentifier);
            PushProp(baseRequest, baseRequest.BankInstanceIdentifier?.ToString(), nameof(baseRequest.BankInstanceIdentifier), AlkamiClaimTypes.BankInstanceIdentifier);
            PushProp(baseRequest, baseRequest.SessionId, "UserSessionId", AlkamiClaimTypes.SessionId);
            return PushProp(baseRequest, baseRequest.BankUri, "BankHostName", AlkamiClaimTypes.BankUrlSignature);

        }

        private IDisposable PushProp(BaseRequest baseRequest, string value, string key, string claimType)
        {
            var claimValue = baseRequest.GetClaimValue(claimType);
            return LogContext.PushProperty(key, string.IsNullOrWhiteSpace(claimValue) ? value : claimValue);
        }
    }
}
#endif
