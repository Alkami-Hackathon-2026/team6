using Alkami.Data.Validations;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="ItemSetting"/>
    /// </summary>
    public class ItemSettingValidator : EntityValidatorImpl<ItemSetting>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(ItemSetting src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(src.Name))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ItemSettingInvalidName });
            }

            return results;
        }
    }
}
