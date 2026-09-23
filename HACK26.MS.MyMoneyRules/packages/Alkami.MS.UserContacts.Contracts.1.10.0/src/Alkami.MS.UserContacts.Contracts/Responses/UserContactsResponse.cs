using Alkami.Contracts;
using Alkami.MS.UserContacts.Data.Dtos;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class UserContactsResponse : BaseResponse
    {
        public UserContactsResponse()
        {
            UserContacts = new List<UserContactBase>();
        }

        [DataMember]
        public IEnumerable<UserContactBase> UserContacts { get; set; }
    }
}
