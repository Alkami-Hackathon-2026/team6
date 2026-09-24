using Alkami.MS.UserContacts.Contracts.Requests;
using Alkami.MS.UserContacts.Contracts.Responses;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Alkami.MS.UserContacts.Contracts
{
    /// <summary>
    /// Provides access to and management for UserContact objects and thier associated UserContactMemberships and UserContactAccounts.
    /// </summary>
    [ServiceContract]
    public interface IUserContactsService
    {
        /// <summary>
        /// Get UserContacts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<UserContactsResponse> GetUserContactsRequestAsync(GetUserContactsRequest request);

        /// <summary>
        /// Add or Update UserContacts.
        /// You cannot delete an entity associated to a UserContact using this method. Look for an explicitly defined delete method to do that for the associated entity type.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<UserContactsResponse> AddOrUpdateUserContactsAsync(AddOrUpdateUserContactsRequest request);

        /// <summary>
        /// Delete UserContacts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<DeleteUserContactsResponse> DeleteUserContactsAsync(DeleteUserContactsRequest request);

        /// <summary>
        /// Delete UserContactMemberships
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<DeleteUserContactMembershipsResponse> DeleteUserContactMembershipsAsync(DeleteUserContactMembershipsRequest request);

        /// <summary>
        /// Delete UserContactAccounts
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<DeleteUserContactAccountsResponse> DeleteUserContactAccountsAsync(DeleteUserContactAccountsRequest request);
    }
}