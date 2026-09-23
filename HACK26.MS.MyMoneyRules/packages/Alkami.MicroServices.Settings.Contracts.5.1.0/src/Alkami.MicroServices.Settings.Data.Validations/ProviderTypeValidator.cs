using Alkami.Data.Validations;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="ProviderType"/>
    /// </summary>
    public class ProviderTypeValidator : EntityValidatorImpl<ProviderType>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(ProviderType src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate });
            }
            else
            {
                src.Providers = src.Providers ?? new List<Provider>();

                if (string.IsNullOrWhiteSpace(src.DisplayName))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ProviderTypeInvalidDisplayName });

                if (string.IsNullOrWhiteSpace(src.Name))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ProviderTypeInvalidName });

                results.AddRange(src.Providers.SelectMany(provider => provider.Validate()));
            }

            return results;
        }
    }
}
