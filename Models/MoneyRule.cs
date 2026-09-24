using System.Text.Json;

namespace AlkamiHackathon.Sandbox.Models;

public sealed class MoneyRule
{
    public string RuleId { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public string? RuleDescription { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public int Priority { get; set; } = 100;

    public string TriggerType { get; set; } = "TRANSFER_INITIATED";

    public List<RuleCondition> Conditions { get; set; } = [];

    public List<RuleAction> Actions { get; set; } = [];

    public string? SourcePrompt { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class RuleCondition
{
    public string FieldKey { get; set; } = string.Empty;

    public string ComparisonOperator { get; set; } = string.Empty;

    public string ValueType { get; set; } = "STRING";

    public JsonElement ComparisonValue { get; set; }
}

public sealed class RuleAction
{
    public string ActionType { get; set; } = string.Empty;

    public Dictionary<string, JsonElement>? ActionConfig { get; set; }
}
