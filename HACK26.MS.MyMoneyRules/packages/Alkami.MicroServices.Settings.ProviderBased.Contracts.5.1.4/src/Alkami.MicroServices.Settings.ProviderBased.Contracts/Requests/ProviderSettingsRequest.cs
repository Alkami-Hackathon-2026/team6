using System.Runtime.Serialization;
using Alkami.Contracts;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts.Requests
{
	[DataContract(IsReference = true)]
	public class ProviderSettingsRequest : BaseRequest
	{
		[DataMember]
		public string ProviderType { get; set; }

		[DataMember]
		public string ProviderName { get; set; }
	}
}
