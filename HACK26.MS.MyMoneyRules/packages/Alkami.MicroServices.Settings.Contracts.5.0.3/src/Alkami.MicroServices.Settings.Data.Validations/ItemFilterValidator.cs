using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="ItemFilter"/>
    /// </summary>
    public class ItemFilterValidator : EntityValidatorImpl<ItemFilter>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(ItemFilter src)
        {
            var results = new List<ValidationResult>();

            if (!String.IsNullOrWhiteSpace(src.ItemType) && src.ItemTypes != null && src.ItemTypes.Any())
            {
                results.Add(new ValidationResult
                {
                    Severity = Severity.Error,
                    ErrorCode = ErrorCode.ValidationError,
                    Message = ValidationErrors.RedundantItemTypeFilterError
                });
            }

            return results;
        }
    }
}
