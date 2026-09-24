namespace AlkamiHackathon.Sandbox.Models;

public sealed class GenerateRuleResponse
{
    public required MoneyRule Rule { get; init; }

    public bool Saved { get; init; }
}
