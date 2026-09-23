using Alkami.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Mappers
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class UserContactMapper : IMapping
    {
        /// <summary>
        /// When set to true, will include any associated UserContactMemberships with each returned UserAccount
        /// </summary>
        [DataMember]
        public bool IncludeUserContactMemberships { get; set; }

        /// <summary>
        /// When set to true, will include any associated UserContactAccounts with each returned UserAccount
        /// </summary>
        [DataMember]
        public bool IncludeUserContactAccounts { get; set; }
    }
}
