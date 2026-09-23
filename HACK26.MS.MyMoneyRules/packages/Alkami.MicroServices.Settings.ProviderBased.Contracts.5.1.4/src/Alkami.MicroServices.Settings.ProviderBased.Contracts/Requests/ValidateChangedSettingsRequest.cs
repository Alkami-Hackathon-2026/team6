using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts.Requests
{
	[DataContract(IsReference = true)]
	public class ValidateChangedSettingsRequest : ProviderSettingsRequest
	{
		[DataMember]
		public Dictionary<string, string> SettingsToValidate { get; set; }
	}
}
