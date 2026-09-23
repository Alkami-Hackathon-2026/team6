using Alkami.Data.Validations;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="Provider"/>
    /// </summary>
    public class ProviderValidator : EntityValidatorImpl<Provider>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(Provider src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(src.Name))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ProviderInvalidName });

                if (string.IsNullOrWhiteSpace(src.AssemblyInfo))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ProviderAssemblyInfoInvalid });
            }

            return results;
        }
    }
}
