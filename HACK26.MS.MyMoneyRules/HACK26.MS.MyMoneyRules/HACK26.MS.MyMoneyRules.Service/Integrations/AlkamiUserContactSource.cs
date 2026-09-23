using Alkami.Contracts;
using Alkami.MS.UserContacts;
using Alkami.MS.UserContacts.Contracts.Mappers;
using Alkami.MS.UserContacts.Contracts.Requests;
using Alkami.MS.UserContacts.Data;
using Alkami.MS.UserContacts.Data.Dtos;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// <see cref="IUserContactSource"/> backed by the Alkami user contacts service
    /// </summary>
    public class AlkamiUserContactSource : IUserContactSource
    {
        private static readonly ILog Logger = LogManager.GetLogger<AlkamiUserContactSource>();

        private readonly UserContactsServiceClient _client;

        /// <summary>
        /// Create a source using a default <see cref="UserContactsServiceClient"/>
        /// </summary>
        public AlkamiUserContactSource() : this(new UserContactsServiceClient(new Version(1, 10)))
        {
        }

        /// <summary>
        /// Create a source using the given client
        /// </summary>
        public AlkamiUserContactSource(UserContactsServiceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public async Task<UserContact> GetUserContactAsync(BaseRequest request, CancellationToken cancellationToken)
        {
            if (request?.UserId == null)
            {
                Logger.Warn($"{nameof(AlkamiUserContactSource)} | No user id on request; contact not retrieved");
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var userId = request.UserId.Value;
            var contactsRequest = new GetUserContactsRequest
            {
                Filter = new UserContactsFilter { UserIds = new List<long> { userId } },
                Mapping = new UserContactMapper()
            };
            contactsRequest.CopyBaseFrom(request);

            var response = await _client.GetUserContactsRequestAsync(contactsRequest).ConfigureAwait(false);

            if (response == null || response.HasError)
            {
                Logger.Error($"{nameof(AlkamiUserContactSource)} | Failed retrieving contacts for user [{userId}] | {response?.SystemMessage}");
                return null;
            }

            var contacts = (response.UserContacts ?? Enumerable.Empty<UserContactBase>()).Where(c => c != null).ToList();

            var email = contacts.OfType<UserContactEmail>()
                .Where(e => !string.IsNullOrWhiteSpace(e.Email) && e.BounceDate == null && e.UserHaltDate == null)
                .OrderByDescending(e => e.IsPrimary)
                .FirstOrDefault();

            var phone = contacts.OfType<UserContactPhone>()
                .Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber) && p.BounceDate == null && p.UserHaltDate == null)
                .OrderByDescending(p => p.CanReceiveSMS)
                .ThenByDescending(p => p.PhoneNumberType == PhoneNumberType.Mobile)
                .ThenByDescending(p => p.IsPrimary)
                .FirstOrDefault();

            if (email == null && phone == null)
            {
                Logger.Warn($"{nameof(AlkamiUserContactSource)} | No usable email or phone for user [{userId}]");
            }

            return new UserContact
            {
                UserId = userId.ToString(CultureInfo.InvariantCulture),
                Email = email?.Email,
                MobilePhone = phone == null
                    ? null
                    : (phone.IsInternationalNumber && !string.IsNullOrWhiteSpace(phone.InternationalNumber) ? phone.InternationalNumber : phone.PhoneNumber)
            };
        }
    }
}
