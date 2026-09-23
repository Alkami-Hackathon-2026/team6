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
using System.IO;
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

        #region JSON file store

        private static readonly object StoreLock = new object();

        /// <summary>
        /// Optional environment variable that overrides the folder containing the table JSON files
        /// </summary>
        private const string JsonPathEnvironmentVariable = "MYMONEYRULES_JSON_PATH";

        private const string RulesFile = "UserEngineRules.json";
        private const string TriggersFile = "UserEngineTriggers.json";
        private const string ConditionGroupsFile = "UserEngineConditionGroups.json";
        private const string ConditionsFile = "UserEngineConditions.json";
        private const string FieldDefinitionsFile = "UserEngineFieldDefinitions.json";
        private const string ActionsFile = "UserEngineActions.json";
        private const string TransactionEventsFile = "UserEngineTransactionEvents.json";
        private const string RuleEvaluationsFile = "UserEngineRuleEvaluations.json";
        private const string ConditionEvaluationsFile = "UserEngineConditionEvaluations.json";
        private const string ActionExecutionsFile = "UserEngineActionExecutions.json";

        private static readonly Lazy<string> JsonDirectory = new Lazy<string>(ResolveJsonDirectory);

        private static string ResolveJsonDirectory()
        {
            var configured = Environment.GetEnvironmentVariable(JsonPathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(configured))
            {
                Directory.CreateDirectory(configured);
                return configured;
            }

            // Walk up from the running service to find the Data project's json folder
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "HACK26.MS.MyMoneyRules.Data", "json");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            var fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        private static List<T> ReadTable<T>(string fileName)
        {
            var path = Path.Combine(JsonDirectory.Value, fileName);
            if (!File.Exists(path))
            {
                return new List<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path)) ?? new List<T>();
        }

        private static void WriteTable<T>(string fileName, IEnumerable<T> rows)
        {
            var path = Path.Combine(JsonDirectory.Value, fileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(rows, Formatting.Indented));
        }

        private static int NextId(IEnumerable<int> ids)
        {
            return ids.DefaultIfEmpty(0).Max() + 1;
        }

        /// <summary>
        /// Reads the rule tables and composes each rule with its triggers, condition groups, conditions and actions
        /// </summary>
        private static List<DecisionRule> LoadRules()
        {
            var rules = ReadTable<DecisionRule>(RulesFile);
            var triggers = ReadTable<RuleTrigger>(TriggersFile);
            var groups = ReadTable<ConditionGroup>(ConditionGroupsFile);
            var conditions = ReadTable<RuleCondition>(ConditionsFile);
            var actions = ReadTable<RuleAction>(ActionsFile);

            foreach (var group in groups)
            {
                group.Conditions = conditions.Where(c => c.ConditionGroupId == group.ConditionGroupId).ToList();
            }

            foreach (var rule in rules)
            {
                rule.Triggers = triggers.Where(t => t.RuleId == rule.RuleId).ToList();
                rule.ConditionGroups = groups.Where(g => g.RuleId == rule.RuleId).ToList();
                rule.Actions = actions.Where(a => a.RuleId == rule.RuleId).ToList();
            }

            return rules;
        }

        /// <summary>
        /// Decomposes the rules back into their flat tables and rewrites each rule table file
        /// </summary>
        private static void SaveRules(List<DecisionRule> rules)
        {
            var groups = rules.SelectMany(r => r.ConditionGroups ?? Enumerable.Empty<ConditionGroup>()).ToList();

            WriteTable(RulesFile, rules.Select(r => new { r.RuleId, r.UserId, r.RuleName, r.Priority, r.IsActive }));
            WriteTable(TriggersFile, rules.SelectMany(r => r.Triggers ?? Enumerable.Empty<RuleTrigger>())
                .Select(t => new { t.TriggerId, t.RuleId, t.FieldName, t.Operator, t.Value }));
            WriteTable(ConditionGroupsFile, groups
                .Select(g => new { g.ConditionGroupId, g.RuleId, g.ParentConditionGroupId, g.LogicOperator }));
            WriteTable(ConditionsFile, groups.SelectMany(g => g.Conditions ?? Enumerable.Empty<RuleCondition>())
                .Select(c => new { c.ConditionId, c.ConditionGroupId, c.FieldName, c.Operator, c.Value }));
            WriteTable(ActionsFile, rules.SelectMany(r => r.Actions ?? Enumerable.Empty<RuleAction>())
                .Select(a => new { a.ActionId, a.RuleId, a.ActionType, a.ActionValue }));
        }

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
                IEnumerable<DecisionRule> query = LoadRules();

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

                    if (!string.IsNullOrWhiteSpace(filter.UserId))
                    {
                        query = query.Where(r => string.Equals(r.UserId, filter.UserId, StringComparison.OrdinalIgnoreCase));
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

                var mapper = request?.Mapping;
                foreach (var rule in query)
                {
                    if (mapper?.IncludeTriggers == false)
                    {
                        rule.Triggers = new List<RuleTrigger>();
                    }

                    if (mapper?.IncludeConditionGroups == false)
                    {
                        rule.ConditionGroups = new List<ConditionGroup>();
                    }

                    if (mapper?.IncludeActions == false)
                    {
                        rule.Actions = new List<RuleAction>();
                    }

                    response.ItemList.Add(rule);
                }
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
                var rules = LoadRules();

                foreach (var rule in request.ItemList.Where(r => r != null))
                {
                    var index = rule.RuleId > 0 ? rules.FindIndex(r => r.RuleId == rule.RuleId) : -1;
                    if (index >= 0)
                    {
                        rules[index] = rule;
                    }
                    else
                    {
                        rule.RuleId = NextId(rules.Select(r => r.RuleId));
                        rules.Add(rule);
                    }

                    AssignChildIds(rule, rules);
                    response.ItemList.Add(rule);
                }

                SaveRules(rules);
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
                var rules = LoadRules();
                var removed = rules.Where(r => request.RuleIds.Contains(r.RuleId)).ToList();

                if (removed.Count > 0)
                {
                    // Rewriting the tables drops the rules' triggers, condition groups, conditions and actions too
                    rules.RemoveAll(r => request.RuleIds.Contains(r.RuleId));
                    SaveRules(rules);
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
                var definitions = ReadTable<FieldDefinition>(FieldDefinitionsFile);
                if (definitions.Count == 0)
                {
                    // Seed the file with the default field catalog
                    definitions = CreateDefaultFieldDefinitions();
                    WriteTable(FieldDefinitionsFile, definitions.Select(f => new { f.FieldId, f.FieldName, f.DataType, f.AllowedOperatorsJson }));
                }

                var names = request?.FieldNames;
                var fields = names == null || names.Count == 0
                    ? definitions
                    : definitions.Where(f => names.Contains(f.FieldName, StringComparer.OrdinalIgnoreCase));

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
                var rules = LoadRules();
                var transactionEvents = ReadTable<TransactionEvent>(TransactionEventsFile);
                var ruleEvaluations = ReadTable<RuleEvaluation>(RuleEvaluationsFile);
                var conditionEvaluations = ReadTable<ConditionEvaluation>(ConditionEvaluationsFile);
                var actionExecutions = ReadTable<ActionExecution>(ActionExecutionsFile);

                if (transactionEvent.EventId <= 0)
                {
                    transactionEvent.EventId = NextId(transactionEvents.Select(e => e.EventId));
                    transactionEvents.Add(transactionEvent);
                }
                else if (transactionEvents.All(e => e.EventId != transactionEvent.EventId))
                {
                    transactionEvents.Add(transactionEvent);
                }

                response.TransactionEvent = transactionEvent;

                var nextEvaluationId = NextId(ruleEvaluations.Select(e => e.EvaluationId));
                var nextConditionEvaluationId = NextId(conditionEvaluations.Select(e => e.ConditionEvaluationId));
                var nextActionExecutionId = NextId(actionExecutions.Select(e => e.ActionExecutionId));

                var activeRules = rules
                    .Where(r => r.IsActive && string.Equals(r.UserId, request.UserId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Priority)
                    .ThenBy(r => r.RuleId)
                    .ToList();

                foreach (var rule in activeRules)
                {
                    var evaluation = new RuleEvaluation
                    {
                        EvaluationId = nextEvaluationId++,
                        EventId = transactionEvent.EventId,
                        RuleId = rule.RuleId,
                        EvaluationDateUtc = DateTime.UtcNow
                    };

                    var conditionResults = new List<ConditionEvaluation>();
                    var triggered = rule.Triggers == null || rule.Triggers.Count == 0
                        || rule.Triggers.All(t => Compare(GetFieldValue(transactionEvent, t.FieldName), t.Operator, t.Value));

                    evaluation.Matched = triggered && EvaluateRuleConditions(rule, transactionEvent, evaluation.EvaluationId, conditionResults, ref nextConditionEvaluationId);

                    ruleEvaluations.Add(evaluation);
                    conditionEvaluations.AddRange(conditionResults);
                    response.ItemList.Add(evaluation);
                    response.ConditionEvaluations.AddRange(conditionResults);

                    if (evaluation.Matched && rule.Actions != null)
                    {
                        foreach (var action in rule.Actions)
                        {
                            var execution = new ActionExecution
                            {
                                ActionExecutionId = nextActionExecutionId++,
                                EvaluationId = evaluation.EvaluationId,
                                ActionId = action.ActionId,
                                Status = "PENDING",
                                ExecutionDateUtc = DateTime.UtcNow
                            };

                            actionExecutions.Add(execution);
                            response.ActionExecutions.Add(execution);
                        }
                    }
                }

                WriteTable(TransactionEventsFile, transactionEvents.Select(e => new
                {
                    e.EventId,
                    e.TransactionOccurred,
                    e.AccountId,
                    e.TransactionId,
                    e.Amount,
                    e.AvailableBalance,
                    e.MerchantName,
                    e.MerchantType,
                    e.TransactionType,
                    e.TransactionDateUtc
                }));
                WriteTable(RuleEvaluationsFile, ruleEvaluations.Select(e => new { e.EvaluationId, e.EventId, e.RuleId, e.Matched, e.EvaluationDateUtc }));
                WriteTable(ConditionEvaluationsFile, conditionEvaluations.Select(e => new
                {
                    e.ConditionEvaluationId,
                    e.EvaluationId,
                    e.ConditionId,
                    e.ActualValue,
                    e.ExpectedValue,
                    e.Result
                }));
                WriteTable(ActionExecutionsFile, actionExecutions.Select(e => new { e.ActionExecutionId, e.EvaluationId, e.ActionId, e.Status, e.ExecutionDateUtc }));
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
                IEnumerable<RuleEvaluation> query = ReadTable<RuleEvaluation>(RuleEvaluationsFile);

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

                var evaluations = query.ToList();
                var mapper = request?.Mapping;

                if (mapper?.IncludeEvent == true)
                {
                    var events = ReadTable<TransactionEvent>(TransactionEventsFile).ToDictionary(e => e.EventId);
                    foreach (var evaluation in evaluations)
                    {
                        evaluation.Event = events.TryGetValue(evaluation.EventId, out var transactionEvent) ? transactionEvent : null;
                    }
                }

                if (mapper?.IncludeRule == true)
                {
                    var rules = LoadRules().ToDictionary(r => r.RuleId);
                    foreach (var evaluation in evaluations)
                    {
                        evaluation.Rule = rules.TryGetValue(evaluation.RuleId, out var rule) ? rule : null;
                    }
                }

                response.ItemList.AddRange(evaluations);
            }

            return await Task.FromResult(response);
        }

        #region Rules engine helpers

        private static void AssignChildIds(DecisionRule rule, List<DecisionRule> allRules)
        {
            var allGroups = allRules.SelectMany(r => r.ConditionGroups ?? Enumerable.Empty<ConditionGroup>()).ToList();

            var nextTriggerId = NextId(allRules.SelectMany(r => r.Triggers ?? Enumerable.Empty<RuleTrigger>()).Select(t => t.TriggerId));
            var nextGroupId = NextId(allGroups.Select(g => g.ConditionGroupId));
            var nextConditionId = NextId(allGroups.SelectMany(g => g.Conditions ?? Enumerable.Empty<RuleCondition>()).Select(c => c.ConditionId));
            var nextActionId = NextId(allRules.SelectMany(r => r.Actions ?? Enumerable.Empty<RuleAction>()).Select(a => a.ActionId));

            rule.Triggers = rule.Triggers ?? new List<RuleTrigger>();
            foreach (var trigger in rule.Triggers)
            {
                trigger.RuleId = rule.RuleId;
                trigger.Rule = null;
                if (trigger.TriggerId <= 0)
                {
                    trigger.TriggerId = nextTriggerId++;
                }
            }

            // Nested ChildGroups are flattened into the rule's ConditionGroups with ParentConditionGroupId set
            var flattened = new List<ConditionGroup>();
            foreach (var group in (rule.ConditionGroups ?? Enumerable.Empty<ConditionGroup>()).ToList())
            {
                AssignGroupIds(group, null, rule, flattened, ref nextGroupId, ref nextConditionId);
            }

            rule.ConditionGroups = flattened;

            rule.Actions = rule.Actions ?? new List<RuleAction>();
            foreach (var action in rule.Actions)
            {
                action.RuleId = rule.RuleId;
                action.Rule = null;
                if (action.ActionId <= 0)
                {
                    action.ActionId = nextActionId++;
                }
            }
        }

        private static void AssignGroupIds(ConditionGroup group, int? parentId, DecisionRule rule, List<ConditionGroup> flattened,
            ref int nextGroupId, ref int nextConditionId)
        {
            if (flattened.Contains(group))
            {
                return;
            }

            group.RuleId = rule.RuleId;
            group.Rule = null;
            group.ParentConditionGroup = null;
            if (group.ConditionGroupId <= 0)
            {
                group.ConditionGroupId = nextGroupId++;
            }

            if (parentId.HasValue)
            {
                group.ParentConditionGroupId = parentId;
            }

            flattened.Add(group);

            group.Conditions = group.Conditions ?? new List<RuleCondition>();
            foreach (var condition in group.Conditions)
            {
                condition.ConditionGroupId = group.ConditionGroupId;
                condition.ConditionGroup = null;
                if (condition.ConditionId <= 0)
                {
                    condition.ConditionId = nextConditionId++;
                }
            }

            foreach (var child in (group.ChildGroups ?? Enumerable.Empty<ConditionGroup>()).ToList())
            {
                AssignGroupIds(child, group.ConditionGroupId, rule, flattened, ref nextGroupId, ref nextConditionId);
            }

            group.ChildGroups = new List<ConditionGroup>();
        }

        private static bool EvaluateRuleConditions(DecisionRule rule, TransactionEvent transactionEvent, int evaluationId,
            List<ConditionEvaluation> results, ref int nextConditionEvaluationId)
        {
            var groups = rule.ConditionGroups?.ToList() ?? new List<ConditionGroup>();
            if (groups.Count == 0)
            {
                return true;
            }

            var groupIds = new HashSet<int>(groups.Select(g => g.ConditionGroupId));
            var rootGroups = groups
                .Where(g => !g.ParentConditionGroupId.HasValue || !groupIds.Contains(g.ParentConditionGroupId.Value))
                .ToList();

            // Root groups are combined with AND
            var matched = true;
            foreach (var root in rootGroups)
            {
                matched &= EvaluateGroup(root, groups, transactionEvent, evaluationId, results, new HashSet<int>(), ref nextConditionEvaluationId);
            }

            return matched;
        }

        private static bool EvaluateGroup(ConditionGroup group, List<ConditionGroup> allGroups, TransactionEvent transactionEvent,
            int evaluationId, List<ConditionEvaluation> results, HashSet<int> visited, ref int nextConditionEvaluationId)
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
                    ConditionEvaluationId = nextConditionEvaluationId++,
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
                outcomes.Add(EvaluateGroup(child, allGroups, transactionEvent, evaluationId, results, visited, ref nextConditionEvaluationId));
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