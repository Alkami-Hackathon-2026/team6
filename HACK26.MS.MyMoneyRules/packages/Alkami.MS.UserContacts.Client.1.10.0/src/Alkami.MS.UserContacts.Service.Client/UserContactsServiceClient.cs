using System;
using System.Threading.Tasks;
using Alkami.MS.UserContacts.Contracts;
using Alkami.MS.UserContacts.Contracts.Requests;
using Alkami.MS.UserContacts.Contracts.Responses;
using Alkami.Services.Subscriptions.ParticipatingClient;

namespace Alkami.MS.UserContacts
{
    /// <inheritdoc/>
    public class UserContactsServiceClient : SelfResolvingClient<IUserContactsService>, IUserContactsService
    {
        /// <summary>
        /// Constructs a UserContactsServiceClient
        /// </summary>
        /// <param name="version"></param>
        public UserContactsServiceClient(Version version = null)
            : base(version)
        {

        }

        /// <inheritdoc/>
        public Task<UserContactsResponse> GetUserContactsRequestAsync(GetUserContactsRequest request)
        {
            return this.ProxyCall((c, r) => c.GetUserContactsRequestAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<UserContactsResponse> AddOrUpdateUserContactsAsync(AddOrUpdateUserContactsRequest request)
        {
            return this.ProxyCall((c, r) => c.AddOrUpdateUserContactsAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteUserContactsResponse> DeleteUserContactsAsync(DeleteUserContactsRequest request)
        {
            return this.ProxyCall((c, r) => c.DeleteUserContactsAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteUserContactMembershipsResponse> DeleteUserContactMembershipsAsync(DeleteUserContactMembershipsRequest request)
        {
            return this.ProxyCall((c, r) => c.DeleteUserContactMembershipsAsync(r), request);
        }

        /// <inheritdoc/>
        public Task<DeleteUserContactAccountsResponse> DeleteUserContactAccountsAsync(DeleteUserContactAccountsRequest request)
        {
            return this.ProxyCall((c, r) => c.DeleteUserContactAccountsAsync(r), request);
        }
    }
}