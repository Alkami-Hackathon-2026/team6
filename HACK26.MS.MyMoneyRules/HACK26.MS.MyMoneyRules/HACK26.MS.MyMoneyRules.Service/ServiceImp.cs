using Alkami.Contracts;
using Alkami.Data.Validations;
using Common.Logging;
using HACK26.MS.MyMoneyRules.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using HACK26.MS.MyMoneyRules.Data;
using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp : IMyMoneyRulesServiceContract
    {
        private static readonly ILog Logger = LogManager.GetLogger<ServiceImp>();

        /// <inheritdoc />
        public async Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            // Create a new response object that encapsulates the data type we'll be returning
            var response = new SettingsResponse();

            // We want some local variables that are available outside of the data scope
            string firstSetting = string.Empty;
            string secondSetting = string.Empty;

            // GetScopeAsync() is how we retrieve settings using the request type of this service
            using (var scope = await GetScopeAsync(request))
            {
                // Assigning the settings to our local variables
                firstSetting = scope.GetSettingOrDefault<string>(SettingNames.FirstProviderSetting);
                secondSetting = scope.GetSettingOrDefault<string>(SettingNames.SecondProviderSetting);
            }

            // It's always good to add a trace log for future troubleshooting
            Logger.Trace($"{nameof(GetSettingsAsync)} | First Setting: [{firstSetting}] | Second Setting: [{secondSetting}]");

            // Populate the details of the Setting object with this service's two template settings
            // The response object's "ItemList" property is an enumerable list of the type we passed into the class definition of SettingsResponse
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.FirstProviderSetting,
                DefaultValue = DefaultSettings()[SettingNames.FirstProviderSetting],
                CurrentValue = firstSetting,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.FirstProviderSetting)?.Description
            });

            // We'll do the same for the second setting, adding another instance of the Setting to the response's ItemList
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.SecondProviderSetting,
                DefaultValue = DefaultSettings()[SettingNames.SecondProviderSetting],
                CurrentValue = secondSetting,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.SecondProviderSetting)?.Description
            });

            // Return the response using an awaited task
            return await Task.FromResult(response);
        }

        #region In-memory rules engine store

        private static readonly object StoreLock = new object();
        private static readonly List<DecisionRule> Rules = new List<DecisionRule>();
        private static readonly List<TransactionEvent> TransactionEvents = new List<TransactionEvent>();
        private static readonly List<RuleEvaluation> RuleEvaluations = new List<RuleEvaluation>();
        private static readonly List<ConditionEvaluation> ConditionEvaluations = new List<ConditionEvaluation>();
        private static readonly List<ActionExecution> ActionExecutions = new List<ActionExecution>();
        private static readonly List<FieldDefinition> FieldDefinitions = CreateDefaultFieldDefinitions();

        private static int _ruleId;
        private static int _triggerId;
        private static int _conditionGroupId;
        private static int _conditionId;
        private static int _actionId;
        private static int _eventId;
        private static int _evaluationId;
        private static int _conditionEvaluationId;
        private static int _actionExecutionId;

        private static List<FieldDefinition> CreateDefaultFieldDefinitions()
        {
            var equality = new List<string> { "=", "!=" };
            var numeric = new List<string> { "=", "!=", ">", ">=", "<", "<=" };
            var text = new List<string> { "=", "!=", "contains", "startsWith", "endsWith", "in" };

            var fieldId = 0;
            return new List<FieldDefinition>
            {
                new FieldDefinition { FieldId = ++fieldId, FieldName = "transactionOccurred", DataType = "bool", AllowedOperators = equality },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "accountId", DataType = "int", AllowedOperators = numeric },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "transactionId", DataType = "long", AllowedOperators = numeric },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "amount", DataType = "decimal", AllowedOperators = numeric },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "availableBalance", DataType = "decimal", AllowedOperators = numeric },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "merchantName", DataType = "string", AllowedOperators = text },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "merchantType", DataType = "string", AllowedOperators = text },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "transactionType", DataType = "string", AllowedOperators = text },
                new FieldDefinition { FieldId = ++fieldId, FieldName = "transactionDateUtc", DataType = "datetime", AllowedOperators = numeric }
            };
        }

        #endregion

        /// <inheritdoc />
        public async Task<DecisionRuleResponse> GetDecisionRulesAsync(GetDecisionRulesRequest request)
        {
            var response = new DecisionRuleResponse();
            var filter = request?.Filter;

            lock (StoreLock)
            {
                IEnumerable<DecisionRule> query = Rules;

                if (filter != null)
                {
                    if (filter.Ids?.Count > 0)
                    {
                        query = query.Where(r => filter.Ids.Contains(r.RuleId));
                    }

                    if (filter.RuleIds?.Count > 0)
                    {
                        query = query.Where(r => filter.RuleIds.Contains(r.RuleId));
                    }

                    if (filter.UserId.HasValue)
                    {
                        query = query.Where(r => r.UserId == filter.UserId.Value);
                    }

                    if (filter.IsActive.HasValue)
                    {
                        query = query.Where(r => r.IsActive == filter.IsActive.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.PartialRuleName))
                    {
                        query = query.Where(r => r.RuleName?.IndexOf(filter.PartialRuleName, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }

                query = query.OrderBy(r => r.Priority).ThenBy(r => r.RuleId);

                if (request?.MaxResults > 0)
                {
                    query = query.Take(request.MaxResults);
                }

                response.ItemList.AddRange(query);
            }

            Logger.Trace($"{nameof(GetDecisionRulesAsync)} | Returned [{response.ItemList.Count}] rules");

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<DecisionRuleResponse> AddOrUpdateDecisionRulesAsync(AddOrUpdateDecisionRuleRequest request)
        {
            var response = new DecisionRuleResponse();

            if (request?.ItemList == null)
            {
                return await Task.FromResult(response);
            }

            lock (StoreLock)
            {
                foreach (var rule in request.ItemList.Where(r => r != null))
                {
                    var existing = Rules.FirstOrDefault(r => r.RuleId == rule.RuleId && rule.RuleId > 0);
                    if (existing != null)
                    {
                        Rules.Remove(existing);
                    }
                    else
                    {
                        rule.RuleId = ++_ruleId;
                    }

                    AssignChildIds(rule);
                    Rules.Add(rule);
                    response.ItemList.Add(rule);
                }
            }

            Logger.Trace($"{nameof(AddOrUpdateDecisionRulesAsync)} | Saved [{response.ItemList.Count}] rules");

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<DecisionRuleResponse> DeleteDecisionRulesAsync(DeleteDecisionRulesRequest request)
        {
            var response = new DecisionRuleResponse();

            if (request?.RuleIds == null || request.RuleIds.Count == 0)
            {
                return await Task.FromResult(response);
            }

            lock (StoreLock)
            {
                var removed = Rules.Where(r => request.RuleIds.Contains(r.RuleId)).ToList();
                foreach (var rule in removed)
                {
                    Rules.Remove(rule);
                }

                response.ItemList.AddRange(removed);
            }

            Logger.Trace($"{nameof(DeleteDecisionRulesAsync)} | Deleted [{response.ItemList.Count}] rules");

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<FieldDefinitionResponse> GetFieldDefinitionsAsync(GetFieldDefinitionsRequest request)
        {
            var response = new FieldDefinitionResponse();

            lock (StoreLock)
            {
                var names = request?.FieldNames;
                var fields = names == null || names.Count == 0
                    ? FieldDefinitions
                    : FieldDefinitions.Where(f => names.Contains(f.FieldName, StringComparer.OrdinalIgnoreCase));

                response.ItemList.AddRange(fields);
            }

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<TransactionEvaluationResponse> EvaluateTransactionAsync(EvaluateTransactionRequest request)
        {
            var response = new TransactionEvaluationResponse();
            var transactionEvent = request?.TransactionEvent;

            if (transactionEvent == null)
            {
                return await Task.FromResult(response);
            }

            lock (StoreLock)
            {
                if (transactionEvent.EventId <= 0)
                {
                    transactionEvent.EventId = ++_eventId;
                }

                TransactionEvents.Add(transactionEvent);
                response.TransactionEvent = transactionEvent;

                var activeRules = Rules
                    .Where(r => r.IsActive && r.UserId == request.UserId)
                    .OrderBy(r => r.Priority)
                    .ThenBy(r => r.RuleId)
                    .ToList();

                foreach (var rule in activeRules)
                {
                    var evaluation = new RuleEvaluation
                    {
                        EvaluationId = ++_evaluationId,
                        EventId = transactionEvent.EventId,
                        RuleId = rule.RuleId,
                        EvaluationDateUtc = DateTime.UtcNow
                    };

                    var conditionResults = new List<ConditionEvaluation>();
                    var triggered = rule.Triggers == null || rule.Triggers.Count == 0
                        || rule.Triggers.All(t => Compare(GetFieldValue(transactionEvent, t.FieldName), t.Operator, t.Value));

                    evaluation.Matched = triggered && EvaluateRuleConditions(rule, transactionEvent, evaluation.EvaluationId, conditionResults);

                    RuleEvaluations.Add(evaluation);
                    ConditionEvaluations.AddRange(conditionResults);
                    response.ItemList.Add(evaluation);
                    response.ConditionEvaluations.AddRange(conditionResults);

                    if (evaluation.Matched && rule.Actions != null)
                    {
                        foreach (var action in rule.Actions)
                        {
                            var execution = new ActionExecution
                            {
                                ActionExecutionId = ++_actionExecutionId,
                                EvaluationId = evaluation.EvaluationId,
                                ActionId = action.ActionId,
                                Status = "PENDING",
                                ExecutionDateUtc = DateTime.UtcNow
                            };

                            ActionExecutions.Add(execution);
                            response.ActionExecutions.Add(execution);
                        }
                    }
                }
            }

            Logger.Trace($"{nameof(EvaluateTransactionAsync)} | Event [{transactionEvent.EventId}] | Evaluated [{response.ItemList.Count}] rules | Matched [{response.ItemList.Count(e => e.Matched)}]");

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<RuleEvaluationResponse> GetRuleEvaluationsAsync(GetRuleEvaluationsRequest request)
        {
            var response = new RuleEvaluationResponse();
            var filter = request?.Filter;

            lock (StoreLock)
            {
                IEnumerable<RuleEvaluation> query = RuleEvaluations;

                if (filter != null)
                {
                    if (filter.Ids?.Count > 0)
                    {
                        query = query.Where(e => filter.Ids.Contains(e.EvaluationId));
                    }

                    if (filter.EvaluationIds?.Count > 0)
                    {
                        query = query.Where(e => filter.EvaluationIds.Contains(e.EvaluationId));
                    }

                    if (filter.RuleIds?.Count > 0)
                    {
                        query = query.Where(e => filter.RuleIds.Contains(e.RuleId));
                    }

                    if (filter.EventIds?.Count > 0)
                    {
                        query = query.Where(e => filter.EventIds.Contains(e.EventId));
                    }

                    if (filter.Matched.HasValue)
                    {
                        query = query.Where(e => e.Matched == filter.Matched.Value);
                    }

                    if (filter.FromDateUtc.HasValue)
                    {
                        query = query.Where(e => e.EvaluationDateUtc >= filter.FromDateUtc.Value);
                    }

                    if (filter.ToDateUtc.HasValue)
                    {
                        query = query.Where(e => e.EvaluationDateUtc <= filter.ToDateUtc.Value);
                    }
                }

                query = query.OrderByDescending(e => e.EvaluationDateUtc);

                if (request?.MaxResults > 0)
                {
                    query = query.Take(request.MaxResults);
                }

                response.ItemList.AddRange(query);
            }

            return await Task.FromResult(response);
        }

        #region Rules engine helpers

        private static void AssignChildIds(DecisionRule rule)
        {
            foreach (var trigger in rule.Triggers ?? Enumerable.Empty<RuleTrigger>())
            {
                trigger.RuleId = rule.RuleId;
                if (trigger.TriggerId <= 0)
                {
                    trigger.TriggerId = ++_triggerId;
                }
            }

            foreach (var group in rule.ConditionGroups ?? Enumerable.Empty<ConditionGroup>())
            {
                group.RuleId = rule.RuleId;
                if (group.ConditionGroupId <= 0)
                {
                    group.ConditionGroupId = ++_conditionGroupId;
                }

                foreach (var condition in group.Conditions ?? Enumerable.Empty<RuleCondition>())
                {
                    condition.ConditionGroupId = group.ConditionGroupId;
                    if (condition.ConditionId <= 0)
                    {
                        condition.ConditionId = ++_conditionId;
                    }
                }
            }

            foreach (var action in rule.Actions ?? Enumerable.Empty<RuleAction>())
            {
                action.RuleId = rule.RuleId;
                if (action.ActionId <= 0)
                {
                    action.ActionId = ++_actionId;
                }
            }
        }

        private static bool EvaluateRuleConditions(DecisionRule rule, TransactionEvent transactionEvent, int evaluationId, List<ConditionEvaluation> results)
        {
            var groups = rule.ConditionGroups?.ToList() ?? new List<ConditionGroup>();
            if (groups.Count == 0)
            {
                return true;
            }

            var groupIds = new HashSet<int>(groups.Select(g => g.ConditionGroupId));
            var rootGroups = groups.Where(g => !groupIds.Contains(g.ParentConditionGroupId)).ToList();

            // Root groups are combined with AND
            var matched = true;
            foreach (var root in rootGroups)
            {
                matched &= EvaluateGroup(root, groups, transactionEvent, evaluationId, results, new HashSet<int>());
            }

            return matched;
        }

        private static bool EvaluateGroup(ConditionGroup group, List<ConditionGroup> allGroups, TransactionEvent transactionEvent,
            int evaluationId, List<ConditionEvaluation> results, HashSet<int> visited)
        {
            if (!visited.Add(group.ConditionGroupId))
            {
                return false;
            }

            var outcomes = new List<bool>();

            foreach (var condition in group.Conditions ?? Enumerable.Empty<RuleCondition>())
            {
                var actual = GetFieldValue(transactionEvent, condition.FieldName);
                var result = Compare(actual, condition.Operator, condition.Value);

                results.Add(new ConditionEvaluation
                {
                    ConditionEvaluationId = ++_conditionEvaluationId,
                    EvaluationId = evaluationId,
                    ConditionId = condition.ConditionId,
                    ActualValue = actual,
                    ExpectedValue = condition.Value,
                    Result = result
                });

                outcomes.Add(result);
            }

            foreach (var child in allGroups.Where(g => g.ParentConditionGroupId == group.ConditionGroupId && g.ConditionGroupId != group.ConditionGroupId))
            {
                outcomes.Add(EvaluateGroup(child, allGroups, transactionEvent, evaluationId, results, visited));
            }

            if (outcomes.Count == 0)
            {
                return true;
            }

            return string.Equals(group.LogicOperator, "OR", StringComparison.OrdinalIgnoreCase)
                ? outcomes.Any(o => o)
                : outcomes.All(o => o);
        }

        private static string GetFieldValue(TransactionEvent transactionEvent, string fieldName)
        {
            switch ((fieldName ?? string.Empty).ToLowerInvariant())
            {
                case "transactionoccurred": return transactionEvent.TransactionOccurred.ToString();
                case "accountid": return transactionEvent.AccountId.ToString(CultureInfo.InvariantCulture);
                case "transactionid": return transactionEvent.TransactionId.ToString(CultureInfo.InvariantCulture);
                case "amount": return transactionEvent.Amount.ToString(CultureInfo.InvariantCulture);
                case "availablebalance": return transactionEvent.AvailableBalance.ToString(CultureInfo.InvariantCulture);
                case "merchantname": return transactionEvent.MerchantName;
                case "merchanttype": return transactionEvent.MerchantType;
                case "transactiontype": return transactionEvent.TransactionType;
                case "transactiondateutc": return transactionEvent.TransactionDateUtc.ToString("o", CultureInfo.InvariantCulture);
                default: return null;
            }
        }

        private static bool Compare(string actual, string op, string expected)
        {
            if (actual == null)
            {
                return op == "!=" && expected != null;
            }

            expected = expected ?? string.Empty;
            var comparison = StringComparison.OrdinalIgnoreCase;

            switch ((op ?? string.Empty).ToLowerInvariant())
            {
                case "contains": return actual.IndexOf(expected, comparison) >= 0;
                case "startswith": return actual.StartsWith(expected, comparison);
                case "endswith": return actual.EndsWith(expected, comparison);
                case "in": return expected.Split(',').Any(v => string.Equals(v.Trim(), actual, comparison));
            }

            int order;
            if (decimal.TryParse(actual, NumberStyles.Any, CultureInfo.InvariantCulture, out var actualNumber)
                && decimal.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedNumber))
            {
                order = actualNumber.CompareTo(expectedNumber);
            }
            else if (bool.TryParse(actual, out var actualBool) && bool.TryParse(expected, out var expectedBool))
            {
                order = actualBool.CompareTo(expectedBool);
            }
            else if (DateTime.TryParse(actual, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var actualDate)
                && DateTime.TryParse(expected, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expectedDate))
            {
                order = actualDate.ToUniversalTime().CompareTo(expectedDate.ToUniversalTime());
            }
            else
            {
                order = string.Compare(actual, expected, comparison);
            }

            switch (op)
            {
                case "=":
                case "==": return order == 0;
                case "!=": return order != 0;
                case ">": return order > 0;
                case ">=": return order >= 0;
                case "<": return order < 0;
                case "<=": return order <= 0;
                default: return false;
            }
        }

        #endregion
    }
}