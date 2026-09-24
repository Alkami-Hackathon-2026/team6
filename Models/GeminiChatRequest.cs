namespace AlkamiHackathon.Sandbox.Models;

public sealed class GeminiChatRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string? SystemInstruction { get; set; }

    /// <summary>
    /// When true (default), asks Gemini for JSON and returns a parsed object in the response.
    /// </summary>
    public bool? ResponseAsJson { get; set; }
}
