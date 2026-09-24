using Alkami.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class DeleteUserContactMembershipsResponse : BaseResponse
    {
        public DeleteUserContactMembershipsResponse()
        {

        }
    }
}
