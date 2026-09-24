using Alkami.Contracts;
using Alkami.Data.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    /// <summary>
    /// The helper class for interacting with microservices.
    /// </summary>
    internal static class MicroserviceHelper
    {
        /// <summary>
        /// Checks for errors in the responses from a microservice.
        /// </summary>
        /// <param name="response">The <see cref="BaseResponse"/> to check.</param>
        /// <param name="callingMethod">The name of the calling method.</param>
        public static void CheckForError(this BaseResponse response,
            [CallerMemberName] string callingMethod = null)
        {
            if (response.HasError)
            {
                var errors = GetErrorsFromResponse(response);

                var errorMessage = string.Format("{0}: {1} failed with: {2}{3}",
                    callingMethod,
                    response.GetType().Name,
                    response.SystemMessage,
                    string.Join(Environment.NewLine, errors.Select(r => r.Message)));

                throw new Exception(errorMessage);
            }
        }

        private static List<ValidationResult> GetErrorsFromResponse(BaseResponse response)
        {
            return response.ValidationResults
                .Where(r => (r.Severity == Severity.Error) || (r.Severity == Severity.Fatal))
                .ToList();
        }
    }
}
