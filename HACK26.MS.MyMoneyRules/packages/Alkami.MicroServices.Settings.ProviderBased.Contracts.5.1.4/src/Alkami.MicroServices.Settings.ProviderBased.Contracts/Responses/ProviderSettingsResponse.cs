using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.Contracts;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts.Responses
{
	[DataContract(IsReference = true)]
	public class ProviderSettingsResponse : BaseResponse
	{
		[DataMember]
		public Dictionary<string, string> ProviderSettings { get; set; }
	}
}
