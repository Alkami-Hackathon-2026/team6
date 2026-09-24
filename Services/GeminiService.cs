using System.Text.Json;
using System.Text.RegularExpressions;
using AlkamiHackathon.Sandbox.Configuration;
using AlkamiHackathon.Sandbox.Models;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace AlkamiHackathon.Sandbox.Services;

public sealed class GeminiService : IGeminiService
{
    private readonly GeminiOptions _options;
    private readonly Client? _client;

    public GeminiService(IOptions<GeminiOptions> options)
    {
        _options = options.Value;
        var apiKey = ResolveApiKey(_options.ApiKey);

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new Client(apiKey: apiKey);
        }
    }

    public bool IsConfigured => _client is not null;

    public string Model => _options.Model;

    public async Task<GeminiChatResponse> GenerateAsync(
        GeminiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            throw new InvalidOperationException(
                "Gemini is not configured. Set Gemini:ApiKey in user secrets or GEMINI_API_KEY in the environment.");
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        var config = new GenerateContentConfig();

        if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
        {
            config.SystemInstruction = new Content
            {
                Parts = [new Part { Text = request.SystemInstruction }]
            };
        }

        var responseAsJson = request.ResponseAsJson ?? true;

        if (responseAsJson)
        {
            config.ResponseMimeType = "application/json";
            config.SystemInstruction = MergeSystemInstruction(
                config.SystemInstruction,
                "Respond with valid JSON only. Use a single JSON object.");
        }

        var response = await _client.Models.GenerateContentAsync(
            model: _options.Model,
            contents: request.Prompt,
            config: config);

        var text = response.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts?
            .FirstOrDefault(part => !string.IsNullOrWhiteSpace(part.Text))?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini returned an empty response.");
        }

        JsonElement? json = null;
        if (responseAsJson)
        {
            try
            {
                using var document = JsonDocument.Parse(ExtractJson(text));
                json = document.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Gemini returned invalid JSON: {ex.Message}", ex);
            }
        }

        return new GeminiChatResponse
        {
            Model = _options.Model,
            Text = responseAsJson ? null : text,
            Json = json
        };
    }

    private static Content MergeSystemInstruction(Content? existing, string instruction)
    {
        var parts = existing?.Parts?.ToList() ?? [];
        parts.Add(new Part { Text = instruction });
        return new Content { Parts = parts };
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        var fenced = Regex.Match(trimmed, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (fenced.Success)
        {
            return fenced.Groups[1].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        var arrayStart = trimmed.IndexOf('[');
        var arrayEnd = trimmed.LastIndexOf(']');
        if (arrayStart >= 0 && arrayEnd > arrayStart)
        {
            return trimmed[arrayStart..(arrayEnd + 1)];
        }

        return trimmed;
    }

    private static string? ResolveApiKey(string? configuredApiKey)
    {
        if (!string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return configuredApiKey;
        }

        return System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }
}
