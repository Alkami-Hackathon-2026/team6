using Alkami.Data.Validations;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using System.Collections.Generic;

namespace HACK26.MS.MyMoneyRules.Data.Validations
{
    public class GeminiChatRequestValidator : EntityValidatorImpl<GeminiChatRequest>
    {
        protected override List<ValidationResult> ValidateInternal(GeminiChatRequest src)
        {
            var results = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(src?.Prompt))
            {
                results.Add(new ValidationResult
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "Prompt",
                    Message = "Prompt is required.",
                    Severity = Severity.Error,
                    SubCode = SubCode.MalformedRequest
                });
            }

            return results;
        }
    }
}
