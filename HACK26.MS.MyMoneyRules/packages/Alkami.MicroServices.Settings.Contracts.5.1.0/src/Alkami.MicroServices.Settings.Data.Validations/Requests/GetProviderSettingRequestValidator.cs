using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Requests;
using System;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations.Requests
{
    /// <summary>
    /// Validates the <see cref="GetProviderSettingRequest"/>
    /// </summary>
    public class GetProviderSettingRequestValidator : EntityValidatorImpl<GetProviderSettingRequest>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(GetProviderSettingRequest src)
        {
            var results = new List<ValidationResult>();

            if (src.ProviderId == 0)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Message = "ProviderId must be a valid id",
                    Severity = Severity.Error
                });
            }

            if (src.BankId == 0)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Message = "BankId must be a valid id",
                    Severity = Severity.Error
                });
            }

            if (String.IsNullOrWhiteSpace(src.ProviderType))
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Message = "ProviderType must be populated",
                    Severity = Severity.Error
                });
            }

            if (String.IsNullOrWhiteSpace(src.ProviderName))
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Message = "ProviderName must be populated",
                    Severity = Severity.Error
                });
            }

            return results;
        }
    }
}
