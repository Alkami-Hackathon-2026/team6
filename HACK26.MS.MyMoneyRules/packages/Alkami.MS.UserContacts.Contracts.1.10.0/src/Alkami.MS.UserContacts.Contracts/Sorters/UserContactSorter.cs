using Alkami.Contracts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Sorters
{
	[ExcludeFromCodeCoverage]
	[DataContract(IsReference = true)]
	public class UserContactSorter : ISortOrder<UserContactSortFields>
	{
		public UserContactSorter()
		{
			Ascending = true;
			OrderByFields = new List<UserContactSortFields>()
            {
                UserContactSortFields.Id
            };
		}

		[DataMember]
		public bool Ascending { get; set; }

		[DataMember]
		public List<UserContactSortFields> OrderByFields { get; set; }
	}
}