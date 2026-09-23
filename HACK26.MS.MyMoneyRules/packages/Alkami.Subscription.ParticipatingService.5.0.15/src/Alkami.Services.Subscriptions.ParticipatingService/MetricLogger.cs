using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Monitoring;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public class MetricLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MetricLogger));

        internal static void HandleResponseError(BaseResponse response, string errorLocation)
        {
            var (defaultValidationError, validationErrors) = response.GetDefaultValidationMessageAndValidationMessagesBySeverity();

            var errorMessage = "Call to {0} failed with {1}";
            string errorCause;

            if (!string.IsNullOrWhiteSpace(response.SystemMessage))
            {
                // A system message was the cause of the failure. Log it.
                errorCause = $"System Message: {response.SystemMessage}";
            }
            else if (!string.IsNullOrWhiteSpace(defaultValidationError))
            {
                // A validation error(s) were the cause of the failure. Log the first one matching the following criteria:
                // 1. Validation Result is Fatal or Error, with Fatal taking precedence
                // 2. Validation Result does not use ErrorCode of Informational or ValidationError
                errorCause = $"Validation Error: {defaultValidationError}";
            }
            else if (validationErrors.Any())
            {
                // If we get here, the following is assumed.
                // 1. HasError = true
                // 2. There is no System Message associated with the error.
                // 3. There is no validation result with Severity Fatal or Error and not of ErrorCode Informational or ValidationError
                // Ergo the failure is a result of validation gone awry (user entering bad data) and should not be sent to new relic
                // Where it would conflate error logging.
                Logger.Debug($"Encountered a Validation Error not matching Severity in (Fatal, Error) and ErrorCode not in (Informational, ValidationError). Errors: {string.Join(Environment.NewLine, validationErrors)}");
                return;
            }
            else
            {
                //The response has an error, but no system message or validation error was supplied. This should be fixed by the dev team for more robust logging.
                Logger.Debug($"Encountered a response of type {response.GetType().Name} in error containing no validation errors or system message. Please fix.");
                errorCause = "No system message or validation errors. Please fix.";
            }

            Metric.NoticeError(string.Format(errorMessage, errorLocation, errorCause), validationErrors);
        }
    }

    internal static class BaseResponseExtensions
    {
        /// <summary>
        /// Returns true if this response is marked as having an error or if it has any error validation results.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static bool HasErrorExtended(this BaseResponse response)
        {
            return response.HasError || (response.ValidationResults != null && response.ValidationResults.Any(x => x.Severity is Severity.Fatal || x.Severity is Severity.Error));
        }

        /// <summary>
        /// Returns a default validation error to pass to New Relic as well as a collection of all validation errors.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static (string, Dictionary<string, string>) GetDefaultValidationMessageAndValidationMessagesBySeverity(this BaseResponse response)
        {
            var validationErrors = response.ValidationResults?.Where(x => x.Severity is Severity.Error || x.Severity is Severity.Fatal).ToList() ?? Enumerable.Empty<ValidationResult>().ToList();

            var firstError = validationErrors
                .GroupBy(x => x.Severity)
                .OrderByDescending(x => x.Key)
                .Select(x => x.FirstOrDefault(y => y.ErrorCode != ErrorCode.Informational && y.ErrorCode != ErrorCode.ValidationError)?.Message)
                .FirstOrDefault();

            var validationErrorsDict = validationErrors
                .GroupBy(x => x.Severity)
                .ToDictionary(x => x.Key.ToString(), x => string.Join(", ", x.Select(y => y.Message)));

            return (firstError, validationErrorsDict);
        }
    }
}
