using Alkami.Contracts;
using Alkami.MS.UserContacts.Data.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class UpdateUserContactsRequest : BaseCreateOrUpdateRequest<UserContactBase>
    {
        public UpdateUserContactsRequest()
        {
            ItemList = new List<UserContactBase>();
        }
    }
}
