using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Requests;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.MicroServices.Settings.Data.Validations.Requests
{
    /// <summary>
    /// Validates the <see cref="AddOrUpdateItemRequest"/>
    /// </summary>
    public class AddOrUpdateItemRequestValidator : EntityValidatorImpl<AddOrUpdateItemRequest>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateItemRequest src)
        {
            var validationResults = new List<ValidationResult>();

            if (src == null)
            {
                validationResults.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.NoEntityWasPresentToValidate });
            }
            else
            {
                src.ItemList = src.ItemList ?? new List<Item>();

                validationResults.AddRange(src.ItemList.SelectMany(entity => entity.Validate()));
            }

            return validationResults;
        }
    }
}
