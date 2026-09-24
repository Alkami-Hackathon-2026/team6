using Alkami.Data.Validations;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Data.Validations
{
    // TODO: AddOrUpdateSomethingRequest was removed; replace with a validator for AddOrUpdateDecisionRuleRequest.
    //public class AddOrUpdateSomethingRequestValidator : EntityValidatorImpl<AddOrUpdateSomethingRequest>
    //{
    //    /// <summary>
    //    /// Override the ValidateInternal method to define custom validations for this particular type of request.
    //    /// </summary>
    //    /// <param name="src"></param>
    //    /// <returns></returns>
    //    protected override List<ValidationResult> ValidateInternal(AddOrUpdateSomethingRequest src)
    //    {
    //        var results = new List<ValidationResult>();
    //
    //        if (src?.ItemList?.Count > 1 || src?.ItemList?.Count < 1)
    //        {
    //            results.Add(new ValidationResult()
    //            {
    //                ErrorCode = ErrorCode.ValidationError,
    //                Field = "ItemList",
    //                Message = "The update item list must have a single value to update.",
    //                Severity = Severity.Error,
    //                SubCode = SubCode.BadRequest
    //            });
    //        }
    //
    //        return results;
    //    }
    //}
}
