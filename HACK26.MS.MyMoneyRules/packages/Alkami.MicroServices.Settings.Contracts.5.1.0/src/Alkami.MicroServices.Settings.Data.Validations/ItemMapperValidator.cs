using System.Collections.Generic;
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Validates the <see cref="ItemMapper"/>
    /// </summary>
    public class ItemMapperValidator : EntityValidatorImpl<ItemMapper>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(ItemMapper src)
        {
            var result = new List<ValidationResult>();

            // For now no validation checkes but added to stop validation items showing up in logs

            return result;
        }
    }
}
