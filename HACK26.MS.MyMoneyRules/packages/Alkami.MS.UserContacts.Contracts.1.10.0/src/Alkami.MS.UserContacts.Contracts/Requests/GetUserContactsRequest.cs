using Alkami.Contracts;
using Alkami.MS.UserContacts.Contracts.Mappers;
using Alkami.MS.UserContacts.Contracts.Sorters;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class GetUserContactsRequest : BaseGetRequest<UserContactsFilter, UserContactMapper, UserContactSorter>
    {
        
    }
}
