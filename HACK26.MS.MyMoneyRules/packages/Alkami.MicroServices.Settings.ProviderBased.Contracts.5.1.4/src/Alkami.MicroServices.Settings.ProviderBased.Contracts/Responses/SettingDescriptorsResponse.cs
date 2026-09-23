using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.Contracts;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts.Responses
{
	[DataContract(IsReference = true)]
	public class SettingDescriptorsResponse : BaseResponse
	{
		[DataMember]
		public List<SettingDescriptor> SettingDescriptors { get; set; }
	}
}
