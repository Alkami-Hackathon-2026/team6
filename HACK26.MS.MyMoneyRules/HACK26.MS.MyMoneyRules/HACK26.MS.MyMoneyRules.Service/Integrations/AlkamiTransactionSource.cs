using Alkami.Contracts;
using Alkami.MicroServices.Transactions.Contracts;
using Alkami.MicroServices.Transactions.Contracts.Requests;
using Alkami.MicroServices.Transactions.Service.Client;
using Common.Logging;
using HACK26.MS.MyMoneyRules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlkamiTransaction = Alkami.MicroServices.Transactions.Data.Transaction;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// <see cref="ITransactionSource"/> backed by the Alkami transactions service
    /// </summary>
    public class AlkamiTransactionSource : ITransactionSource
    {
        private static readonly ILog Logger = LogManager.GetLogger<AlkamiTransactionSource>();

        private const int PageSize = 500;

        private readonly TransactionServiceClient _client;

        /// <summary>
        /// Create a source using a default <see cref="TransactionServiceClient"/>
        /// </summary>
        public AlkamiTransactionSource() : this(new TransactionServiceClient())
        {
        }

        /// <summary>
        /// Create a source using the given client
        /// </summary>
        public AlkamiTransactionSource(TransactionServiceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public async Task<IList<TransactionEvent>> GetTransactionsAsync(BaseRequest request, IList<int> accountIds, DateTime sinceUtc, CancellationToken cancellationToken)
        {
            var results = new List<TransactionEvent>();
            if (accountIds == null || accountIds.Count == 0)
            {
                return results;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var transactionsRequest = new GetTransactionsRequest
            {
                Filter = new TransactionFilter
                {
                    AccountIds = accountIds.Select(id => (long)id).ToList(),
                    DateSearchField = TransactionDateFields.PostingDate,
                    // Posting dates are typically date-only; already-evaluated transactions are de-duplicated by the worker
                    StartDate = sinceUtc.Date,
                    IsVoid = false
                },
                Mapping = new TransactionMapper { IncludeTransactionType = true }
            };
            transactionsRequest.CopyBaseFrom(request);
            transactionsRequest.MaxResults = PageSize;

            var response = await _client.GetTransactionsAsync(transactionsRequest).ConfigureAwait(false);

            if (response == null || response.HasError)
            {
                Logger.Error($"{nameof(AlkamiTransactionSource)} | Failed retrieving transactions for user [{request?.UserId}] | {response?.SystemMessage}");
                return results;
            }

            results.AddRange((response.Transactions ?? new List<AlkamiTransaction>())
                .Where(t => t != null)
                .Select(Map));

            Logger.Debug($"{nameof(AlkamiTransactionSource)} | Retrieved [{results.Count}] transactions for user [{request?.UserId}]");
            return results;
        }

        private static TransactionEvent Map(AlkamiTransaction transaction)
        {
            var enrichment = transaction.Enrichments?.FirstOrDefault(e => e != null);

            return new TransactionEvent
            {
                TransactionOccurred = true,
                AccountId = (int)transaction.AccountId,
                TransactionId = transaction.Id,
                Amount = Math.Abs(transaction.Amount),
                AvailableBalance = transaction.Balance ?? 0m,
                MerchantName = FirstNonEmpty(enrichment?.Merchant, enrichment?.CleansedDescription, transaction.GeneralDescription, transaction.SpecificDescription),
                MerchantType = transaction.TransactionType?.Name,
                TransactionType = transaction.Debit ? "Debit" : "Credit",
                TransactionDateUtc = DateTime.SpecifyKind(transaction.EffectiveDate ?? transaction.PostingDate, DateTimeKind.Utc)
            };
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
        }
    }
}
