using Alkami.Contracts;
using Alkami.MS.UserContacts.Data.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class DeleteUserContactAccountsRequest : BaseCreateOrUpdateRequest<UserContactAccount>
    {
        public DeleteUserContactAccountsRequest()
        {
            ItemList = new List<UserContactAccount>();
        }
    }
}
