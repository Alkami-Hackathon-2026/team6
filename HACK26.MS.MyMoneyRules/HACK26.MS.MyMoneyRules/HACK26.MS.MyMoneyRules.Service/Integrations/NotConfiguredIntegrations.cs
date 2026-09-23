using Alkami.Contracts;
using Common.Logging;
using HACK26.MS.MyMoneyRules.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// Placeholder integrations used until the Alkami service clients are wired in. Each logs a warning and does nothing.
    /// </summary>
    public class NotConfiguredIntegrations : ITransactionSource, IUserContactSource, INotificationSender
    {
        private static readonly ILog Logger = LogManager.GetLogger<NotConfiguredIntegrations>();

        /// <inheritdoc />
        public Task<IList<TransactionEvent>> GetTransactionsAsync(BaseRequest request, IList<int> accountIds, DateTime sinceUtc, CancellationToken cancellationToken)
        {
            Logger.Warn($"{nameof(ITransactionSource)} is not configured; no transactions retrieved for [{accountIds.Count}] accounts");
            return Task.FromResult<IList<TransactionEvent>>(new List<TransactionEvent>());
        }

        /// <inheritdoc />
        public Task<UserContact> GetUserContactAsync(BaseRequest request, CancellationToken cancellationToken)
        {
            Logger.Warn($"{nameof(IUserContactSource)} is not configured; no contact retrieved for user [{request.UserId}]");
            return Task.FromResult<UserContact>(null);
        }

        /// <inheritdoc />
        public Task<bool> SendAsync(BaseRequest request, UserContact contact, string channel, string message, CancellationToken cancellationToken)
        {
            Logger.Warn($"{nameof(INotificationSender)} is not configured; notification not sent to user [{request.UserId}]");
            return Task.FromResult(false);
        }
    }
}
