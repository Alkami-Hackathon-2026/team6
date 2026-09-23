using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Sorters
{
	[DataContract]
	public enum UserContactSortFields
	{
		[EnumMember]
		Id = 0,

		[EnumMember]
        DisplayName,

		[EnumMember]
        LastUpdate,

		[EnumMember]
		CreateDate
	}
}