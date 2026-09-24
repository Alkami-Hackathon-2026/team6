using Alkami.Data.Validations;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="ProviderSetting"/>
    /// </summary>
    public class ProviderSettingValidator : EntityValidatorImpl<ProviderSetting>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(ProviderSetting src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate, Severity = Severity.Error });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(src.Name))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ProviderSettingInvalidName, Severity = Severity.Error });
            }

            return results;
        }
    }
}
