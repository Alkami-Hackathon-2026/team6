using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.Data;
using Newtonsoft.Json;
using SettingDescriptor = Alkami.MicroServices.Settings.ProviderBased.Contracts.SettingDescriptor;

namespace Alkami.TrackableObjects
{
    /// <summary>
    /// Helper methods used to build errors messages and retrieve values from settings dictionaries
    /// </summary>
    public static class SettingValidationUtility
    {
        #region Error Handling

        /// <summary>
        /// An error message that is encapsulated by an invalid operation exception
        /// when a Key is passed to a validate method, but does not exist in the list
        /// of setting descriptor's
        /// </summary>
        public const string KeyNotFoundMessage =
            "The Key '{0}' being validated could not be found in the list of settings for {1}";

        #endregion Error Handling

        #region Public Validators

        /// <summary>
        /// Validates that a long is between a minimum and maximum value
        /// </summary>
        /// <param name="valueToValidate">The datetime to validate</param>
        /// <param name="minValue">The minimum value the value must be greater than</param>
        /// <param name="maxValue">The maximum value the value must be lessthan</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the validation is suce</returns>
        public static bool IsBetweenRange(this long valueToValidate, long minValue, long maxValue,
            out string error)
        {
            error = string.Empty;

            if (valueToValidate < minValue)
            {
                error = string.Format("This Setting must be greater than {0}", minValue);

                return (false);
            }

            if (valueToValidate > maxValue)
            {
                error = string.Format("This Setting must be less than {0}", maxValue);

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates that an int is between a minimum and maximum value
        /// </summary>
        /// <param name="valueToValidate">The datetime to validate</param>
        /// <param name="minValue">The minimum value the value must be greater than</param>
        /// <param name="maxValue">The maximum value the value must be lessthan</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the validation is suce</returns>
        public static bool IsBetweenRange(this int valueToValidate, int minValue, int maxValue,
            out string error)
        {
            error = string.Empty;

            if (valueToValidate < minValue)
            {
                error = string.Format("This Setting must be greater than {0}", minValue);

                return (false);
            }

            if (valueToValidate > maxValue)
            {
                error = string.Format("This Setting must be less than {0}", maxValue);

                return (false);
            }

            return (true);
        }

		/// <summary>
		/// Validates that an int is greater than a minimum value
		/// </summary>
		/// <param name="valueToValidate">The datetime to validate</param>
		/// <param name="minValue">The minimum value the value must be greater than</param>
		/// <returns>a boolean value indicating if the validation is successful</returns>
		public static bool IsGreaterThan(this int valueToValidate, int minValue, out string error)
        {
            error = string.Empty;

            if (valueToValidate < minValue)
            {
                error = string.Format("This Setting must be greater than {0}", minValue);

                return (false);
            }

            return (true);
        }

		/// <summary>
		/// Validates that an long is greater than a minimum value
		/// </summary>
		/// <param name="valueToValidate">The datetime to validate</param>
		/// <param name="minValue">The minimum value the value must be greater than</param>
		/// <param name="error">An error message if the validation failed</param>
		/// <returns>a boolean value indicating if the validation is successful</returns>
		public static bool IsGreaterThan(this long valueToValidate, long minValue, out string error)
        {
            error = string.Empty;

            if (valueToValidate < minValue)
            {
                error = string.Format("This Setting must be greater than {0}", minValue);

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates that a decimal is greater than or equal to a minimum value
        /// </summary>
        /// <param name="valueToValidate">The datetime to validate</param>
        /// <param name="minValue">The minimum value the value must be greater than</param>
        /// <param name="isInclusive">if set to true the min value is included</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the validation is successful</returns>
        public static bool IsGreaterThan(this decimal valueToValidate, decimal minValue,
            bool isInclusive, out string error)
        {
            error = string.Empty;

            if (!isInclusive)
            {
                if (valueToValidate < minValue)
                {
                    error = string.Format("This Setting must be greater than {0}", minValue);

                    return (false);
                }
            }
            else
            {
                if (valueToValidate <= minValue)
                {
                    error = string.Format("This Setting must be greater than or equal to {0}", minValue);

                    return (false);
                }
            }

            return (true);
        }

        /// <summary>
        /// Validates that a decimal is greater than or equal to a minimum value
        /// </summary>
        /// <param name="valueToValidate">The datetime to validate</param>
        /// <param name="minValue">The minimum value the value must be greater than</param>
        /// <param name="isInclusive">if set to true the min value is included</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the validation is successful</returns>
        public static bool IsGreaterThan(this double valueToValidate, double minValue,
            bool isInclusive, out string error)
        {
            error = string.Empty;

            if (!isInclusive)
            {
                if (valueToValidate < minValue)
                {
                    error = string.Format("This Setting must be greater than {0}", minValue);

                    return (false);
                }
            }
            else
            {
                if (valueToValidate <= minValue)
                {
                    error = string.Format("This Setting must be greater than or equal to {0}", minValue);

                    return (false);
                }
            }

            return (true);
        }

        /// <summary>
        /// Performs the default validation for a given type, if one exists.
        /// </summary>
        /// <param name="type">The type of value we are validating.</param>
        /// <param name="settingValue">The string value we are validating</param>
        /// <param name="errorMessage">The error message if the validation fails</param>
        /// <param name="isValidated">A flag that determines if we validated the type passed in</param>
        /// <returns>a value that indicates if there were any errors.</returns>
        public static bool PerformDefaultTypeValidation(string type, string settingValue,
            out string errorMessage, out bool isValidated)
        {
            var hasErrors = false;
            isValidated = false;
            errorMessage = string.Empty;

            // Do default validation based upon the type
            switch (type)
            {
                case "System.Int32":
                    hasErrors = !settingValue.ValidateIntSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Boolean":
                    hasErrors = !settingValue.ValidateBoolSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Int64":
                    hasErrors = !settingValue.ValidateLongSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Decimal":
                    hasErrors = !settingValue.ValidateDecimalSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Uri":
                    hasErrors = !settingValue.ValidateUriSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.DateTime":
                    hasErrors = !settingValue.ValidateDateTimeSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Net.IPAddress":
                    hasErrors = !settingValue.ValidateIpAddressSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.String":
                    isValidated = true;
                    break;

                case "System.Byte":
                    hasErrors = !settingValue.ValidateByteSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Char":
                    hasErrors = !settingValue.ValidateCharSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Double":
                    hasErrors = !settingValue.ValidateDoubleSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Single":
                    hasErrors = !settingValue.ValidateFloatSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.SByte":
                    hasErrors = !settingValue.ValidateSByteSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Int16":
                    hasErrors = !settingValue.ValidateShortSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.UInt32":
                    hasErrors = !settingValue.ValidateUnsignedIntSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.UInt64":
                    hasErrors = !settingValue.ValidateUnsignedLongSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.UInt16":
                    hasErrors = !settingValue.ValidateUnsignedShortSetting(out errorMessage);
                    isValidated = true;
                    break;

                case "System.Guid":
                    hasErrors = !settingValue.ValidateGuidSetting(out errorMessage);
                    isValidated = true;
                    break;
            }

            return (hasErrors);
        }

        /// <summary>
        /// Validates the conversion to a bool from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateBoolSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            bool parsedValue;
            var canParse = bool.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a boolean value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a byte from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateByteSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            byte parsedValue;
            var canParse = byte.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a byte value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a char from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateCharSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            char parsedValue;
            var canParse = char.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a char value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a date time from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateDateTimeSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            DateTime parsedValue;
            var canParse = DateTime.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a Datetime";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates that a date time is between a minimum and maximum date
        /// </summary>
        /// <param name="dateTimeToValidate">The datetime to validate</param>
        /// <param name="minDate">The minimum value the value must be greater than</param>
        /// <param name="maxDate">The maximum value the value must be lessthan</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the validation is suce</returns>
        public static bool ValidateDateTimeSettingIsInRange(this DateTime dateTimeToValidate,
            DateTime minDate, DateTime maxDate, out string error)
        {
            error = string.Empty;

            if (dateTimeToValidate < minDate)
            {
                error = string.Format("This DateTime Setting must be greater than {0}", minDate);
                return (false);
            }

            if (dateTimeToValidate > maxDate)
            {
                error = string.Format("This DateTime Setting must be less than {0}", maxDate);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a decimal from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateDecimalSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            decimal parsedValue;
            var canParse = decimal.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a decimal value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a double from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateDoubleSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            double parsedValue;
            var canParse = double.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a double value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to an enum from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateEnumSetting<TEnum>(this string settingToValidate, out string error)
            where TEnum : struct
        {
            error = string.Empty;

            TEnum parsedValue;
            var canParse = Enum.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = string.Format("Unable to parse setting as an enum of type {0}", typeof(TEnum));

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a float
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateFloatSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            float parsedValue;
            var canParse = float.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a float value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a valid guid from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        public static bool ValidateGuidSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            Guid guidResult;
            var canParse = Guid.TryParse(settingToValidate, out guidResult);

            if (!canParse)
            {
                error = "Unable to parse setting as a valid guid";

                return (false);
            }
            return (true);
        }

        /// <summary>
        /// Validates the conversion to an int from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateIntSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            int parsedValue;
            var canParse = int.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as an int value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a valid guid from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        public static bool ValidateIpAddressSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            IPAddress ipaddress;
            var canParse = IPAddress.TryParse(settingToValidate, out ipaddress);

            if (!canParse)
            {
                error = "Unable to parse setting as a valid IPAddress";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a valid object from a json string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateJsonSetting<T>(this string settingToValidate, out string error)
        {
            error = string.Empty;

            try
            {
                JsonConvert.DeserializeObject<T>(settingToValidate);
            }
            catch (Exception ex)
            {
                error = string.Format("Unable to Parse JSON string to type {0}, Exception is {1}", typeof(T), ex);

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a long from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateLongSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            long parsedValue;
            var canParse = long.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a long value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates that Required Settings have defaults when they are deleted
        /// </summary>
        /// <param name="trackableEntityBase">The base object</param>
        /// <param name="settingsToChange">The settings that are changing</param>
        /// <param name="errors">A dictionary of errors</param>
        /// <returns>a boolean value that indicates if the validation is successful</returns>
        public static bool ValidateRequiredSettingsHaveDefaultsWhenDeleted(
            this ConfigurationDrivenObject trackableEntityBase, Dictionary<string, string> settingsToChange,
            out List<ValidationResult> errors)
        {
            errors = new List<ValidationResult>();

            foreach (var kvp in settingsToChange)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    continue;
                }

                // If its a required settings, and there is no default settings, and we
                // are deleting the setting, then we have an error condition
                if (trackableEntityBase.SettingDescriptors().Any(x => x.Name == kvp.Key && x.IsRequired) &&
                    !trackableEntityBase.DefaultSettings().ContainsKey(kvp.Key))
                {
                    errors.Add(
                               new ValidationResult()
                               {
                                   Message = "A Required Setting is being removed, but has no default setting to fall back on",
                                   Severity = Severity.Error,
                                   Field = kvp.Key
                               }
                        );
                }
            }

            return errors.Any();
        }

        /// <summary>
        /// Validates that Required Settings have defaults when they are deleted
        /// </summary>
        /// <param name="settingDescriptors">A list of settings</param>
        /// <param name="defaultSettings">A Dictionary of default settings and their values</param>
        /// <param name="settingsToChange">The settings that are being changed</param>
        /// <param name="errors">A dictionary of errors</param>
        /// <returns>a boolean value that indicates if the validation is successful</returns>
        public static bool ValidateRequiredSettingsHaveDefaultsWhenDeleted(
            List<SettingDescriptor> settingDescriptors, Dictionary<string, string> defaultSettings,
            Dictionary<string, string> settingsToChange, out List<ValidationResult> errors)
        {
            errors = new List<ValidationResult>();

            foreach (var kvp in settingsToChange)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    continue;
                }

                // If its a required settings, and there is no default settings, and we
                // are deleting the setting, then we have an error condition
                if (settingDescriptors.Any(x => x.Name == kvp.Key && x.IsRequired) &&
                    !defaultSettings.ContainsKey(kvp.Key))
                {
                    errors.Add(
                               new ValidationResult
                               {
                                   Message =
                                       "A Required Setting is being removed, but has no default setting to fall back on",
                                   Severity = Severity.Error,
                                   Field = kvp.Key
                               }
                        );
                }
            }
            return errors.Any();
        }

        /// <summary>
        /// Validates the conversion to a sbyte from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>

        public static bool ValidateSByteSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            sbyte parsedValue;
            var canParse = sbyte.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a sbyte value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a short from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateShortSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            short parsedValue;
            var canParse = short.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a short value";

                return (false);
            }

            return (true);
        }

		/// <summary>
		/// Validates that a string setting is not null or empty, or optionally just whitespace
		/// </summary>
		/// <param name="settingToValidate">The string to validate</param>
		/// <param name="error">The error message being returned</param>
		/// <returns></returns>
		public static bool ValidateStringSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            if (settingToValidate == null)
            {
                error = "String cannot be null or emtpy";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to an unsigned int from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateUnsignedIntSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            uint parsedValue;
            var canParse = uint.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a unsigned int value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to an unsigned long from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateUnsignedLongSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            ulong parsedValue;
            var canParse = ulong.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a unsigned long value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to an unsigned short from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateUnsignedShortSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            ushort parsedValue;
            var canParse = ushort.TryParse(settingToValidate, out parsedValue);

            if (!canParse)
            {
                error = "Unable to parse setting as a unsigned short value";

                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Validates the conversion to a valid http or https url from a string
        /// </summary>
        /// <param name="settingToValidate">The setting to validate</param>
        /// <param name="error">An error message if the validation failed</param>
        /// <returns>a boolean value indicating if the setting could be validated</returns>
        public static bool ValidateUriSetting(this string settingToValidate, out string error)
        {
            error = string.Empty;

            Uri uriResult;
            var canParse = Uri.TryCreate(settingToValidate, UriKind.Absolute, out uriResult) &&
                           (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!canParse)
            {
                error = "Unable to parse setting as a valid http or https URL";

                return (false);
            }

            return (true);
        }
        #endregion Public Validators
    }
}