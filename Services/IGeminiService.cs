using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public interface IGeminiService
{
    bool IsConfigured { get; }

    string Model { get; }

    Task<GeminiChatResponse> GenerateAsync(GeminiChatRequest request, CancellationToken cancellationToken = default);
}
