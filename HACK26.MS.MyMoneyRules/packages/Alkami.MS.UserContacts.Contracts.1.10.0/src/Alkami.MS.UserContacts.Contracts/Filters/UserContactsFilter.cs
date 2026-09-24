using Alkami.Contracts;
using Alkami.MS.UserContacts.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Alkami.MS.UserContacts.Contracts.Requests
{
    [ExcludeFromCodeCoverage]
    [DataContract(IsReference = true)]
    public class UserContactsFilter : IFilter
    {
        /// <summary>
        /// When set, filters the result collection by the contact types and by the value (optional) of those contacts.
        /// The values supported for filtering are: number for Phone, address for Email, normalized address for Address, and ChannelIdentifier for PushNotificationChannel.
        /// If no value is included with a contact type then the collection is still filtered by that contact type.
        /// </summary>
        [DataMember]
        public IEnumerable<KeyValuePair<int, string>> ContactTypeValues { get; set; }

        /// <summary>
        /// When set, filters the result collection by the PhoneNumber types of the User Contacts by excluding the results that match.
        /// </summary>
        [DataMember]
        public IEnumerable<int> ExcludedPhoneNumberTypes { get; set; }

        /// <summary>
        /// When set, filters the result collection by the Address types of the User Contacts by excluding the results that match.
        /// </summary>
        [DataMember]
        public IEnumerable<int> ExcludedAddressTypes { get; set; }

        /// <summary>
        /// When set, filters the result collection to only those contacts created after a specific date
        /// </summary>
        [DataMember]
        public DateTime? CreatedAfterDate { get; set; }

        /// <summary>
        /// If any ids are present, filters the result collection to only those contacts with these specific ids
        /// </summary>
        [DataMember]
        public List<long> Ids { get; set; }

        /// <summary>
        /// When set, filters the result collection to only those contacts that have a CoreSynced state matching the value
        /// </summary>
        [DataMember]
        public bool? IsCoreSynced { get; set; }

        /// <summary>
        /// When set, filters the result collection to only those contacts that have a primary state matching the value
        /// </summary>
        [DataMember]
        public bool? IsPrimary { get; set; }

        [DataMember]
        public List<long> UserIds { get; set; }

        /// <summary>
        /// When set, filters the result collection to only those contacts that have a modified date after this value
        /// </summary>
        [DataMember]
        public DateTime? ModifiedAfterDate { get; set; }

        /// <summary>
        /// When set (not null or empty), filters the result collection to only those contacts where the display name contains this text
        /// </summary>
        [DataMember]
        public string PartialDisplayName { get; set; }

        /// <summary>
        /// If any Ids are present, filters the result collection to those contacts with these specific ProviderIds. 
        /// </summary>
        [DataMember]
        public List<long> ProviderIds { get; set; }
    }
}