using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public interface IGeminiRuleService
{
    Task<GenerateRuleResponse> GenerateRuleAsync(
        GenerateRuleRequest request,
        CancellationToken cancellationToken = default);
}
