using System.Collections.Generic;
using Alkami.Data.Validations;
using System.Linq;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="Item"/>
    /// </summary>
    public class ItemValidator : EntityValidatorImpl<Item>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(Item src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate });
            }
            else
            {
                src.ItemSettings = src.ItemSettings ?? new List<ItemSetting>();

                if (src.ParentId == 0)
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ItemInvalidParentId });

                if (string.IsNullOrWhiteSpace(src.Name))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ItemInvalidName });

                if (string.IsNullOrWhiteSpace(src.ItemType))
                    results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.ItemInvalidItemType });

                results.AddRange(src.ItemSettings.SelectMany(itemSetting => itemSetting.Validate()));
            }

            return results;
        }
    }
}
