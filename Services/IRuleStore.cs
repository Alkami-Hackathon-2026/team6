using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public interface IRuleStore
{
    IReadOnlyList<MoneyRule> GetRules();

    IReadOnlyList<MoneyRule> GetRulesByMemberId(string memberId);

    MoneyRule? GetRuleById(string ruleId);

    MoneyRule AddRule(MoneyRule rule);

    MoneyRule UpdateRule(MoneyRule rule);

    bool DeleteRule(string ruleId);
}
