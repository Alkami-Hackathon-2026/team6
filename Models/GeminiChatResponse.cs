using System.Text.Json;

namespace AlkamiHackathon.Sandbox.Models;

public sealed class GeminiChatResponse
{
    public required string Model { get; init; }

    public string? Text { get; init; }

    public JsonElement? Json { get; init; }
}
