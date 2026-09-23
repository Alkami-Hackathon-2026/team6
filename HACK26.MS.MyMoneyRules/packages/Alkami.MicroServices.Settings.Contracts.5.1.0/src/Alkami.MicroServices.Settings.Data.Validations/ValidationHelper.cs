using Alkami.Data.Validations;
using System.Collections.Generic;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// Helper functions for the Validation objects
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Run Validate for <paramref name="sourceObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static List<ValidationResult> Validate<T>(this T sourceObject)
        {
            var validationResults = new List<ValidationResult>();

            try
            {
                sourceObject.Validate(out validationResults);
            }
            catch
            {
                validationResults.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Message = ValidationErrors.UnknownValidationError });
            }

            return validationResults;
        }
    }
}
