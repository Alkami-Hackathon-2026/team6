using Alkami.Contracts;
using Common.Logging;
using HACK26.MS.MyMoneyRules.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Filters;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Data;
using HACK26.MS.MyMoneyRules.Service.Integrations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <summary>
    /// Background worker that runs <see cref="DoWorkAsync"/> on a fixed interval (every minute by default)
    /// </summary>
    public class RulesBackgroundWorker : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger<RulesBackgroundWorker>();

        private readonly TimeSpan _interval;
        private readonly IMyMoneyRulesServiceContract _rulesService;
        private readonly ITransactionSource _transactions;
        private readonly IUserContactSource _userContacts;
        private readonly INotificationSender _notifications;
        private readonly object _sync = new object();
        private CancellationTokenSource _cancellation;
        private Timer _timer;
        private int _running;
        private DateTime _lastRunUtc;

        /// <summary>
        /// Create a worker that fires every minute
        /// </summary>
        public RulesBackgroundWorker(IMyMoneyRulesServiceContract rulesService, ITransactionSource transactions,
            IUserContactSource userContacts, INotificationSender notifications)
            : this(rulesService, transactions, userContacts, notifications, TimeSpan.FromMinutes(1))
        {
        }

        /// <summary>
        /// Create a worker that fires on the given interval
        /// </summary>
        public RulesBackgroundWorker(IMyMoneyRulesServiceContract rulesService, ITransactionSource transactions,
            IUserContactSource userContacts, INotificationSender notifications, TimeSpan interval)
        {
            _rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _userContacts = userContacts ?? throw new ArgumentNullException(nameof(userContacts));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            _interval = interval;
            _lastRunUtc = DateTime.UtcNow - interval;
        }

        /// <summary>
        /// Start the timer. The first run happens after one interval.
        /// </summary>
        public void Start()
        {
            lock (_sync)
            {
                if (_timer != null)
                {
                    return;
                }

                _cancellation = new CancellationTokenSource();
                _timer = new Timer(OnTick, null, _interval, _interval);
            }

            Logger.Info($"{nameof(RulesBackgroundWorker)} started | Interval [{_interval}]");
        }

        /// <summary>
        /// Stop the timer and signal any running work to cancel
        /// </summary>
        public void Stop()
        {
            lock (_sync)
            {
                if (_timer == null)
                {
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;

                _cancellation.Cancel();
                _cancellation.Dispose();
                _cancellation = null;
            }

            Logger.Info($"{nameof(RulesBackgroundWorker)} stopped");
        }

        private async void OnTick(object state)
        {
            // Skip this tick if the previous run hasn't finished
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                Logger.Warn($"{nameof(RulesBackgroundWorker)} | Previous run still in progress, skipping tick");
                return;
            }

            try
            {
                CancellationToken token;
                lock (_sync)
                {
                    if (_cancellation == null)
                    {
                        return;
                    }

                    token = _cancellation.Token;
                }

                await DoWorkAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.Trace($"{nameof(RulesBackgroundWorker)} | Run cancelled");
            }
            catch (Exception ex)
            {
                // Never let an exception escape a timer callback; it would crash the host
                Logger.Error($"{nameof(RulesBackgroundWorker)} | Run failed", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        /// <summary>
        /// The work performed on each tick
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var runStartedUtc = DateTime.UtcNow;

            // 1. Active rules, grouped by owning user
            var rulesResponse = await _rulesService.GetDecisionRulesAsync(new GetDecisionRulesRequest
            {
                Filter = new DecisionRuleFilter { IsActive = true }
            }).ConfigureAwait(false);

            var rulesByUser = rulesResponse.ItemList
                .Where(r => r.UserId > 0)
                .GroupBy(r => r.UserId)
                .ToList();

            if (rulesByUser.Count == 0)
            {
                Logger.Trace($"{nameof(RulesBackgroundWorker)} | No active rules");
                _lastRunUtc = runStartedUtc;
                return;
            }

            var evaluatedTransactionIds = ServiceImp.GetEvaluatedTransactionIds();

            foreach (var userRules in rulesByUser)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ProcessUserAsync(userRules.Key, userRules.ToList(), evaluatedTransactionIds, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // One user's failure shouldn't stop processing for the others
                    Logger.Error($"{nameof(RulesBackgroundWorker)} | Processing failed for user [{userRules.Key}]", ex);
                }
            }

            _lastRunUtc = runStartedUtc;
        }

        private async Task ProcessUserAsync(int userId, List<DecisionRule> rules, HashSet<long> evaluatedTransactionIds, CancellationToken cancellationToken)
        {
            // 2. Account ids configured on the user's active rules
            var accountIds = GetConfiguredAccountIds(rules);
            if (accountIds.Count == 0)
            {
                Logger.Trace($"{nameof(RulesBackgroundWorker)} | User [{userId}] has no rules configured with account ids");
                return;
            }

            var baseRequest = CreateBaseRequest(userId);

            // 3. Transactions for those accounts from the Alkami transactions service
            var transactions = await _transactions.GetTransactionsAsync(baseRequest, accountIds, _lastRunUtc, cancellationToken).ConfigureAwait(false);
            var newTransactions = (transactions ?? new List<TransactionEvent>())
                .Where(t => t != null && evaluatedTransactionIds.Add(t.TransactionId))
                .ToList();

            Logger.Trace($"{nameof(RulesBackgroundWorker)} | User [{userId}] | Accounts [{string.Join(",", accountIds)}] | New transactions [{newTransactions.Count}]");

            UserContact contact = null;
            var contactLoaded = false;
            var actionsById = rules.SelectMany(r => r.Actions ?? Enumerable.Empty<RuleAction>()).ToDictionary(a => a.ActionId);

            foreach (var transaction in newTransactions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 4. Apply the active rules
                var evaluation = await _rulesService.EvaluateTransactionAsync(new EvaluateTransactionRequest
                {
                    TransactionEvent = transaction,
                    UserId = userId
                }).ConfigureAwait(false);

                var statusUpdates = new Dictionary<int, string>();

                foreach (var execution in evaluation.ActionExecutions)
                {
                    if (!actionsById.TryGetValue(execution.ActionId, out var action) || !IsNotification(action))
                    {
                        continue;
                    }

                    // 5. User contact info, loaded once per user when the first notification is needed
                    if (!contactLoaded)
                    {
                        contact = await _userContacts.GetUserContactAsync(baseRequest, cancellationToken).ConfigureAwait(false);
                        contactLoaded = true;
                    }

                    if (contact == null)
                    {
                        statusUpdates[execution.ActionExecutionId] = "FAILED";
                        continue;
                    }

                    // 6. Send the notification
                    GetNotificationDetails(action, transaction, out var channel, out var message);
                    var sent = await _notifications.SendAsync(baseRequest, contact, channel, message, cancellationToken).ConfigureAwait(false);
                    statusUpdates[execution.ActionExecutionId] = sent ? "SUCCESS" : "FAILED";
                }

                ServiceImp.UpdateActionExecutionStatuses(statusUpdates);
            }
        }

        /// <summary>
        /// Account ids come from condition AccountIds, and from rule triggers or conditions on the accountId field using "=" or "in"
        /// </summary>
        private static List<int> GetConfiguredAccountIds(IEnumerable<DecisionRule> rules)
        {
            var accountIds = new HashSet<int>();

            foreach (var rule in rules)
            {
                foreach (var condition in (rule.ConditionGroups ?? Enumerable.Empty<ConditionGroup>())
                    .SelectMany(g => g.Conditions ?? Enumerable.Empty<RuleCondition>()))
                {
                    accountIds.UnionWith(condition.GetAccountIdList());
                }

                var criteria = (rule.Triggers ?? Enumerable.Empty<RuleTrigger>())
                    .Select(t => new { t.FieldName, t.Operator, t.Value })
                    .Concat((rule.ConditionGroups ?? Enumerable.Empty<ConditionGroup>())
                        .SelectMany(g => g.Conditions ?? Enumerable.Empty<RuleCondition>())
                        .Select(c => new { c.FieldName, c.Operator, c.Value }));

                foreach (var criterion in criteria)
                {
                    if (!string.Equals(criterion.FieldName, "accountId", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrWhiteSpace(criterion.Value))
                    {
                        continue;
                    }

                    var op = (criterion.Operator ?? string.Empty).Trim().ToLowerInvariant();
                    if (op != "=" && op != "==" && op != "in")
                    {
                        continue;
                    }

                    foreach (var value in criterion.Value.Split(','))
                    {
                        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var accountId))
                        {
                            accountIds.Add(accountId);
                        }
                    }
                }
            }

            return accountIds.OrderBy(id => id).ToList();
        }

        private static BaseRequest CreateBaseRequest(int userId)
        {
            return new GetDecisionRulesRequest { UserId = userId };
        }

        private static bool IsNotification(RuleAction action)
        {
            return action.ActionType?.IndexOf("Notification", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// ActionValue is expected to be JSON like {"channel":"push","message":"..."}; plain text is used as the message
        /// </summary>
        private static void GetNotificationDetails(RuleAction action, TransactionEvent transaction, out string channel, out string message)
        {
            channel = "push";
            message = null;

            if (!string.IsNullOrWhiteSpace(action.ActionValue))
            {
                try
                {
                    var value = JObject.Parse(action.ActionValue);
                    channel = (string)value["channel"] ?? channel;
                    message = (string)value["message"];
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    message = action.ActionValue;
                }
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = string.Format(CultureInfo.InvariantCulture, "{0} transaction of {1:C} at {2}",
                    transaction.TransactionType, transaction.Amount, transaction.MerchantName ?? "unknown merchant");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }
    }
}
