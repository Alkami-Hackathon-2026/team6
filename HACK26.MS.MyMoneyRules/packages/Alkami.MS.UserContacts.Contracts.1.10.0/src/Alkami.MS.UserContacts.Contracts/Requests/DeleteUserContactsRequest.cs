using Alkami.Contracts;
using Alkami.MS.UserContacts.Data.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class DeleteUserContactsRequest : BaseCreateOrUpdateRequest<UserContactBase>
    {
        public DeleteUserContactsRequest()
        {
            ItemList = new List<UserContactBase>();
        }
    }
}
