using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Requests;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations.Requests
{
    /// <summary>
    /// Validates the <see cref="GetBanksRequest"/>
    /// </summary>
    public class GetBanksRequestValidator : EntityValidatorImpl<GetBanksRequest>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(GetBanksRequest src)
        {
            return new List<ValidationResult>()
            {
                new ValidationResult()
                {
                    ErrorCode = ErrorCode.Informational,
                    Message = "Get Bank Request Validation is missing",
                    Severity = Severity.Warning
                }
            };
        }
    }
}
