namespace AlkamiHackathon.Sandbox.Models;

public sealed class GenerateRuleRequest
{
    public string MemberId { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public bool Save { get; set; } = true;
}
