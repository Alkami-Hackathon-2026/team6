namespace AlkamiHackathon.Sandbox.Configuration;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>
    /// API key from Google AI Studio (https://aistudio.google.com/apikey).
    /// Can also be set via the GEMINI_API_KEY environment variable.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Model id for the free Gemini Developer API tier.
    /// </summary>
    public string Model { get; set; } = "gemini-3.6-flash";
}
