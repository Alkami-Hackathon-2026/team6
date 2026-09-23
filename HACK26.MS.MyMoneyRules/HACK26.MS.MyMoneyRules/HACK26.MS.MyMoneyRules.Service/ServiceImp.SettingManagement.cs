using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBased.Contracts;
using Alkami.TrackableObjects.Plugins;
using HACK26.MS.MyMoneyRules.Data.Gemini;
using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using System.Collections.Generic;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp
    {
        /// <summary>
        /// Create default settings that the service expects when first being deployed, these can be anything you'd like them to be
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> DefaultSettings()
        {
            var settings = base.DefaultSettings() ?? new Dictionary<string, string>();

            if (!settings.ContainsKey(SettingNames.FirstProviderSetting))
            {
                settings.Add(SettingNames.FirstProviderSetting, "First Provider Setting");
            }

            if (!settings.ContainsKey(SettingNames.SecondProviderSetting))
            {
                settings.Add(SettingNames.SecondProviderSetting, "Second Provider Setting");
            }

            if (!settings.ContainsKey(SettingNames.GeminiApiKey))
            {
                settings.Add(SettingNames.GeminiApiKey, string.Empty);
            }

            if (!settings.ContainsKey(SettingNames.GeminiModel))
            {
                settings.Add(SettingNames.GeminiModel, GeminiDefaults.DefaultModel);
            }

            return settings;
        }

        /// <summary>
        /// These descriptors need to be added in order to properly display them in the Admin Portal
        /// Use the SettingDescriptor constructor to create a new descriptor for each setting name    
        /// </summary>
        /// <returns></returns>
        public override List<SettingDescriptor> SettingDescriptors()
        {
            var descriptors = new List<SettingDescriptor>();

            // We'll add the range of default setting descriptors, this is most likely empty
            descriptors.AddRange(base.SettingDescriptors());

            // Next add a descriptor for each of the settings
            descriptors.Add(new SettingDescriptor(SettingNames.FirstProviderSetting, "A meaningful description of the value, so that administrators of the configuration can know why they are changing this value.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.SecondProviderSetting, "A meaningful description of the value, so that administrators of the configuration can know why they are changing this value.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.GeminiApiKey, "API key from Google AI Studio (https://aistudio.google.com/apikey). Can also be set via the GEMINI_API_KEY environment variable.", typeof(string), true, "Gemini Api Key", true));
            descriptors.Add(new SettingDescriptor(SettingNames.GeminiModel, "Model id for the free Gemini Developer API tier.", typeof(string), true, "Gemini Model", false));

            return descriptors;
        }

        /// <summary>
        /// Each setting should be validated as well as possible. This will safeguard the implementation from incorrect configuration.
        /// </summary>
        /// <param name="settingDescriptor">The setting Descriptor to validate</param>
        /// <param name="settingValue">The value to validate</param>
        /// <param name="errors">Pre-initialized collection of errors for you to add any additional errors to</param>
        /// <param name="isValidated">A flag that determines if the SettingDescriptor type has been validated; it will be false only if the default type validation fails.</param>
        /// <param name="performedValidation">Set this to true if validation checks have been performed</param>
        protected override void ValidateChangedSetting(SettingDescriptor settingDescriptor, string settingValue, List<ValidationResult> errors, bool isValidated, ref bool performedValidation)
        {
            if (!isValidated) return;

            switch (settingDescriptor.Name)
            {
                case SettingNames.FirstProviderSetting:
                    {
                        // The developer should decide how best to validate the changed settings, these two validations demonstrate the pattern you should use for your settings.
                        // For this example, we're going to make sure that the FirstProviderSetting value contains the word "First"
                        // This makes sure that the Admin user cannot save a value that is not "valid"
                        if (!settingValue.Contains("First"))
                        {
                            Logger.Error(
                                $"{settingDescriptor.Name} value requires that the word `First` exist as part of the string value. [{settingValue}]");
                            errors.AddValidationError(SettingNames.FirstProviderSetting,
                                "Value must contain the word `Second`", SubCode.ValueUnsupported);
                        }

                        performedValidation = true;
                        break;
                    }
                case SettingNames.SecondProviderSetting:
                    {
                        // We will do the same for the SecondProviderSetting, check if the value contains the word "Second"
                        // Again, this is up to the developers discretion on how best to validate
                        if (!settingValue.Contains("Second"))
                        {
                            Logger.Error(
                                $"{settingDescriptor.Name} value requires that the word `Second` exist as part of the string value. [{settingValue}]");
                            errors.AddValidationError(SettingNames.SecondProviderSetting,
                                "Value must contain the word `Second`", SubCode.ValueUnsupported);
                        }

                        performedValidation = true;
                        break;
                    }
                case SettingNames.GeminiApiKey:
                    {
                        performedValidation = true;
                        break;
                    }
                case SettingNames.GeminiModel:
                    {
                        if (string.IsNullOrWhiteSpace(settingValue))
                        {
                            errors.AddValidationError(SettingNames.GeminiModel,
                                "Gemini model is required.", SubCode.ValueUnsupported);
                        }

                        performedValidation = true;
                        break;
                    }
                default:
                    {
                        Logger.Info(
                            $"{settingDescriptor.Name} value was not validated! Has it been defined within the DefaultSettings class and has it been added to the SettingsDescriptors collections?");
                        performedValidation = false;
                        break;
                    }
            }
        }
    }
}