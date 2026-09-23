using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alkami.MicroServices.Settings.Data.Validations.Requests
{
    /// <summary>
    /// Validates the <see cref="AddOrUpdateProviderSettingRequest"/>
    /// </summary>
    public class AddOrUpdateProviderSettingRequestValidator : EntityValidatorImpl<AddOrUpdateProviderSettingRequest>
    {
        ///<inheritdoc />
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateProviderSettingRequest src)
        {
            var results = new List<ValidationResult>();

            if (src.ItemList?.Count > 0)
            {
                var itemId = src.ItemList[0].ItemId;

                // Move to validator
                if (src.ItemList.Any(x => x.ItemId != itemId))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "ItemId",
                        Message = "AddOrUpdateProviderSettingRequest only supports one item at a time.",
                        Severity = Severity.Error,
                        SubCode = SubCode.ValueUnsupported
                    });
                }

                if (String.IsNullOrWhiteSpace(src.CommitMessage))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "CommitMessage",
                        Message = "AddOrUpdateProviderSettingRequest requires a commit message.",
                        Severity = Severity.Error,
                        SubCode = SubCode.ValueUnsupported
                    });
                }

                if (String.IsNullOrWhiteSpace(src.ProviderType))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Message = "ProviderType must be populated",
                        Severity = Severity.Error
                    });
                }

                if (String.IsNullOrWhiteSpace(src.ProviderName))
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Message = "ProviderName must be populated",
                        Severity = Severity.Error
                    });
                }
            }
            else
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "ItemId",
                    Message = "You must provide items to update.",
                    Severity = Severity.Error,
                    SubCode = SubCode.ValueUnsupported
                });
            }

            return results;
        }
    }
}
