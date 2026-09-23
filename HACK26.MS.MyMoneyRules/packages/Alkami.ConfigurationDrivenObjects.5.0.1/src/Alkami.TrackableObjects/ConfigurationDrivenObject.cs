using Alkami.Contracts;
using Alkami.Data.Validations;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using SettingDescriptor = Alkami.MicroServices.Settings.ProviderBased.Contracts.SettingDescriptor;

namespace Alkami.TrackableObjects
{
	/// <summary>
	///
	/// </summary>
	public abstract class ConfigurationDrivenObject
	{
		public List<string> Names { get; set; }

		public virtual string Name { get { return Names?.First(); } set { Names = new List<string> { value }; } }

		/// <summary>
		/// InternalLogger
		/// </summary>
		protected internal ILog InternalLogger;

		/// <summary>
		///
		/// </summary>
		public abstract string ItemType { get; }
		
		public async Task<TenantSpecificScope> GetScopeAsync(BaseRequest request)
		{
			var scope = new TenantSpecificScope(this, request);
			await scope.CreateScopeAsync();
			return scope;
		}

		/// <summary>
		/// Used to return a list of case sensitive settings that are required for this
		/// implementation to work
		/// </summary>
		/// <returns>The required settings</returns>
		public abstract List<SettingDescriptor> SettingDescriptors();

		/// <summary>
		/// Determines if the settings and their values are valid in the context of this
		/// implementation
		/// </summary>
		/// <param name="settingsToValidate">The setting(s) you want to validate</param>
		/// <param name="errors">
		/// Any errors (if any) with the setting name as the key and the error code/message
		/// as the value. This map must be set to a valid, non-null instance.
		/// </param>
		/// <returns></returns>
		public bool ValidateChangedSettings(
			Dictionary<string, string> settingsToValidate,
			out List<ValidationResult> errors)
		{
			var combinedErrors = new List<ValidationResult>();

			var settingDescriptors = SettingDescriptors();
			var hasError = SettingValidationUtility.ValidateRequiredSettingsHaveDefaultsWhenDeleted(
				settingDescriptors, DefaultSettings(), settingsToValidate, out errors);

			if (hasError)
				return (false);

			foreach (KeyValuePair<string, string> kvp in settingsToValidate)
			{
				string settingName = kvp.Key;
				string settingValue = kvp.Value;

				// If the setting value is empty, it's about to be deleted. No further
				// validation is necessary.
				if (String.IsNullOrWhiteSpace(settingValue))
					continue;

				SettingDescriptor settingDescriptor = settingDescriptors.FirstOrDefault(s =>
					s.Name == settingName);

				if (settingDescriptor == null)
				{
					AddError(errors, settingName, "Setting was not found in settings " +
						"descriptor list");

					continue;
				}

				string errorMessage;
				bool isValidated;

				bool hasErrors = SettingValidationUtility.PerformDefaultTypeValidation(
					settingDescriptor.Type, settingValue, out errorMessage, out isValidated);

				if (hasErrors)
				{
					AddError(errors, settingName, errorMessage);

					// Continue here because further validation is moot. If the string
					// value cannot be parsed correctly, then any further validation
					// will fail as well.
					continue;
				}

				var performedValidation = true;

				ValidateChangedSetting(settingDescriptor, settingValue, errors,
					isValidated, ref performedValidation);

				// AaronM: Do not remove or change this logging statement without
				// updating the unit test(s)!
				if (!performedValidation)
				{
					InternalLogger.WarnFormat("No Validator Found for {0}",
						settingDescriptor.Name);
				}
			}

			combinedErrors.AddRange(errors);

			errors = combinedErrors;

			return !errors.Any(x => x.Severity == Severity.Error || x.Severity == Severity.Fatal);
		}

		public virtual async Task<ItemFilter> GetFilterAsync(BaseRequest request, string providerName = null)
		{
			return new ItemFilter()
			{
				ItemType = ItemType,
				ParentIds = new List<long>()
							{
								await GetParentIdAsync(request)
							},
				SecondaryIds = new List<long>()
							{
								await GetSecondaryIdAsync(request)
							}
			};
		}

		public abstract Dictionary<string, string> DefaultSettings();

		/// <summary>
		/// Gets the parent Id of an item
		/// </summary>
		/// <param name="parentRequest">The request that needs the parent Id</param>
		/// <returns>The parent ID of the item</returns>
		protected abstract Task<long> GetParentIdAsync(BaseRequest parentRequest);

		/// <summary>
		/// Gets the secondary Id of an item
		/// </summary>
		/// <param name="parentRequest">The request that needs the secondary Id</param>
		/// <returns>The secondary ID of the item</returns>
		protected abstract Task<long> GetSecondaryIdAsync(BaseRequest parentRequest);

		protected abstract void ValidateChangedSetting(SettingDescriptor settingDescriptor, string settingValue, List<ValidationResult> errors, bool isValidated, ref bool performedValidation);

		private void AddError(List<ValidationResult> errors, string settingName, string errorMessage)
		{
			errors.Add(new ValidationResult()
			{
				ErrorCode = ErrorCode.ValidationError,
				Field = settingName,
				Message = errorMessage
			});
		}
	}
}