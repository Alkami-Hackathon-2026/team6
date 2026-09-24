using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// Retrieves transactions for accounts. Wrap the Alkami transactions service client in an implementation of this interface.
    /// </summary>
    public interface ITransactionSource
    {
        /// <summary>
        /// Get transactions posted to the given accounts since <paramref name="sinceUtc"/>, mapped to <see cref="TransactionEvent"/>
        /// </summary>
        Task<IList<TransactionEvent>> GetTransactionsAsync(BaseRequest request, IList<int> accountIds, DateTime sinceUtc, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Contact details used to deliver a notification
    /// </summary>
    public class UserContact
    {
        /// <summary>User identifier</summary>
        public string UserId { get; set; }

        /// <summary>Email address</summary>
        public string Email { get; set; }

        /// <summary>Mobile phone number</summary>
        public string MobilePhone { get; set; }
    }

    /// <summary>
    /// Retrieves user contact information. Wrap the Alkami user/contact service client in an implementation of this interface.
    /// </summary>
    public interface IUserContactSource
    {
        /// <summary>
        /// Get contact information for the user identified on <paramref name="request"/>
        /// </summary>
        Task<UserContact> GetUserContactAsync(BaseRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Sends notifications. Wrap the Alkami notification service client in an implementation of this interface.
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Send a notification to the user; returns true when accepted by the notification service
        /// </summary>
        Task<bool> SendAsync(BaseRequest request, UserContact contact, string channel, string message, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Generates text using a Gemini model. Implementations must be thread-safe; a single instance is shared across requests.
    /// </summary>
    public interface IGeminiClient
    {
        /// <summary>
        /// Generate text for <paramref name="prompt"/> using the per-request <paramref name="options"/>
        /// </summary>
        Task<GeminiResult> GenerateAsync(GeminiOptions options, string prompt, string systemInstruction, CancellationToken cancellationToken);
    }
}
