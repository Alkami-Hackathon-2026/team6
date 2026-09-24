#if NETFRAMEWORK
using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Exceptions;
using Alkami.Monitoring;
using Alkami.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Dispatcher;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public partial class BaseErrorHandler : IOperationInvoker
    {
        public bool IsSynchronous
        {
            get { return _innerInvoker.IsSynchronous; }
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            try
            {
                var validationResult = this.PerformValidation(inputs);

                if (validationResult.HasError)
                {
                    outputs = new object[] { validationResult };

                    return validationResult;
                }
                else
                {
                    var invokeResponse = _innerInvoker.Invoke(instance, inputs, out outputs) as BaseResponse;

                    // Add any warnings produced during validation to the service's response.
                    invokeResponse.AddValidationResultsFrom(validationResult);

                    outputs = new object[] { invokeResponse };

                    return invokeResponse;
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
                outputs = new object[] { result };
                return result;
            }
        }

        delegate object InvokerDelegate(object[] inputs, out object[] outputs);

        private void InvokerCallback(IAsyncResult asyncResult)
        {
            ValidationUserState validationUserState = asyncResult.AsyncState as ValidationUserState;
            validationUserState.OriginalUserCallback(new BaseErrorHandlerAsyncResult(asyncResult, validationUserState));
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            var validationResult = this.PerformValidation(inputs);

            var result = new BaseErrorHandlerResult { ReturnValue = validationResult, Outputs = new object[] { } };
            ValidationUserState validationUserState = new ValidationUserState
            {
                ValidationResult = result,
                OriginalUserCallback = callback,
                OriginalUserState = state
            };

            IAsyncResult originalAsyncResult;
            if (validationResult.HasError)
            {
                InvokerDelegate invoker = result.GetValue;
                object[] dummy;
                originalAsyncResult = invoker.BeginInvoke(inputs, out dummy, this.InvokerCallback, validationUserState);
            }
            else
            {
                originalAsyncResult = this._innerInvoker.InvokeBegin(instance, inputs, this.InvokerCallback, validationUserState);
            }

            return new BaseErrorHandlerAsyncResult(originalAsyncResult, validationUserState);

        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult arResult)
        {
            var baseErrorHandlerAsyncResult = arResult as BaseErrorHandlerAsyncResult;
            var validationUserState = baseErrorHandlerAsyncResult.ValidationUserState;
            try
            {

                if (validationUserState.ValidationResult.ReturnValue.HasError)
                {
                    outputs = new object[] { };
                    return validationUserState.ValidationResult.ReturnValue;
                }
                else
                {
                    var result = baseErrorHandlerAsyncResult.OriginalAsyncResult as System.Threading.Tasks.Task;
                    if (result != null && result.IsFaulted && result.Exception != null)
                    {
                        return HandleException(validationUserState, result.Exception, out outputs);
                    }

                    var invokeResult = _innerInvoker.InvokeEnd(instance, out outputs, baseErrorHandlerAsyncResult.OriginalAsyncResult);
                    var invokeResponse = invokeResult as BaseResponse;

                    if (invokeResponse != null)
                        invokeResponse.AddValidationResultsFrom(validationUserState.ValidationResult.ReturnValue);

                    return invokeResult;
                }
            }
            catch (Exception e)
            {
                return HandleException(validationUserState, e, out outputs);
            }
        }

        private object HandleException(ValidationUserState validationUserState, Exception e, out object[] outputs)
        {
            Metric.NoticeError(e);

            Logger.Error("Unhandled error during InvokeEnd:", e);
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
            result.ValidationResults.AddRange(validationUserState.ValidationResult.ReturnValue.ValidationResults);
            result.SystemMessage = alk.Message;
            outputs = new[] { (object)result };
            return result;
        }

        public IDisposable AddBaseRequestPropertiesToLogContext(BaseRequest baseRequest)
        {
            if (baseRequest == null)
                return null;

            PushProp(baseRequest, baseRequest.UserIdentifier?.ToString(), nameof(baseRequest.UserIdentifier), AlkamiClaimTypes.UserIdentifier);
            PushProp(baseRequest, baseRequest.BankIdentifier?.ToString(), nameof(baseRequest.BankIdentifier), AlkamiClaimTypes.BankIdentifier);
            PushProp(baseRequest, baseRequest.BankInstanceIdentifier?.ToString(), nameof(baseRequest.BankInstanceIdentifier), AlkamiClaimTypes.BankInstanceIdentifier);
            PushProp(baseRequest, baseRequest.SessionId, "UserSessionId", AlkamiClaimTypes.SessionId);
            PushProp(baseRequest, baseRequest.BankUri, "BankHostName", AlkamiClaimTypes.BankUrlSignature);

            return null;
        }

        private IDisposable PushProp(BaseRequest baseRequest, string value, string key, string claimType)
        {
            var claimValue = baseRequest.GetClaimValue(claimType);
            log4net.LogicalThreadContext.Properties[key] = string.IsNullOrWhiteSpace(claimValue) ? value : claimValue;

            return null;
        }
    }
}
#endif
