using System.Text.Json;
using System.Text.Json.Serialization;
using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public sealed class JsonRuleStore : IRuleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _rulesPath;
    private readonly object _sync = new();
    private List<MoneyRule> _rules;

    public JsonRuleStore(IWebHostEnvironment environment)
    {
        _rulesPath = Path.Combine(environment.ContentRootPath, "Data", "rules.json");
        _rules = LoadRules();
    }

    public IReadOnlyList<MoneyRule> GetRules() => _rules;

    public IReadOnlyList<MoneyRule> GetRulesByMemberId(string memberId) =>
        _rules.Where(r => r.MemberId.Equals(memberId, StringComparison.OrdinalIgnoreCase)).ToList();

    public MoneyRule? GetRuleById(string ruleId) =>
        _rules.FirstOrDefault(r => r.RuleId.Equals(ruleId, StringComparison.OrdinalIgnoreCase));

    public MoneyRule AddRule(MoneyRule rule)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleId))
            {
                rule.RuleId = $"rule-{Guid.NewGuid():N}"[..12];
            }

            if (_rules.Any(r => r.RuleId.Equals(rule.RuleId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Rule '{rule.RuleId}' already exists.");
            }

            var now = DateTime.UtcNow;
            rule.CreatedAtUtc = rule.CreatedAtUtc == default ? now : rule.CreatedAtUtc;
            rule.UpdatedAtUtc = now;

            _rules.Add(rule);
            SaveRules();
            return rule;
        }
    }

    public MoneyRule UpdateRule(MoneyRule rule)
    {
        lock (_sync)
        {
            var index = _rules.FindIndex(r => r.RuleId.Equals(rule.RuleId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new KeyNotFoundException($"Rule '{rule.RuleId}' was not found.");
            }

            rule.UpdatedAtUtc = DateTime.UtcNow;
            _rules[index] = rule;
            SaveRules();
            return rule;
        }
    }

    public bool DeleteRule(string ruleId)
    {
        lock (_sync)
        {
            var removed = _rules.RemoveAll(r => r.RuleId.Equals(ruleId, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                SaveRules();
                return true;
            }

            return false;
        }
    }

    private List<MoneyRule> LoadRules()
    {
        if (!File.Exists(_rulesPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_rulesPath)!);
            File.WriteAllText(_rulesPath, "[]");
            return [];
        }

        var json = File.ReadAllText(_rulesPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<MoneyRule>>(json, JsonOptions) ?? [];
    }

    private void SaveRules()
    {
        var json = JsonSerializer.Serialize(_rules, JsonOptions);
        File.WriteAllText(_rulesPath, json);
    }
}
