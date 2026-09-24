using System.Text.Json;
using System.Text.RegularExpressions;
using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public sealed partial class GeminiRuleService : IGeminiRuleService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IGeminiService _geminiService;
    private readonly ISandboxDataStore _dataStore;
    private readonly IRuleStore _ruleStore;

    public GeminiRuleService(
        IGeminiService geminiService,
        ISandboxDataStore dataStore,
        IRuleStore ruleStore)
    {
        _geminiService = geminiService;
        _dataStore = dataStore;
        _ruleStore = ruleStore;
    }

    public async Task<GenerateRuleResponse> GenerateRuleAsync(
        GenerateRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MemberId))
        {
            throw new ArgumentException("MemberId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        var profile = _dataStore.GetMemberProfile(request.MemberId)
            ?? throw new KeyNotFoundException($"Member '{request.MemberId}' was not found.");

        var shareContext = profile.Shares
            .Select(s => new
            {
                s.Share.ShareId,
                s.Share.Nickname,
                s.Share.ShareType,
                s.Share.AvailableBalance,
                s.Share.AllowTransfersIn,
                s.Share.AllowTransfersOut
            });

        var systemInstruction = """
            You create money management rules for a credit union demo API.
            Return ONLY valid JSON matching this schema (no markdown, no commentary):
            {
              "ruleName": "string",
              "ruleDescription": "string",
              "status": "ACTIVE",
              "priority": 100,
              "triggerType": "TRANSFER_INITIATED",
              "conditions": [
                {
                  "fieldKey": "transfer.amount",
                  "comparisonOperator": "GTE",
                  "valueType": "DECIMAL",
                  "comparisonValue": 500
                }
              ],
              "actions": [
                {
                  "actionType": "SEND_PUSH_NOTIFICATION",
                  "actionConfig": { "message": "string" }
                }
              ]
            }

            Allowed fieldKey values: transfer.amount, transfer.fromShareId, transfer.toShareId,
            transfer.toShareType, share.availableBalance, share.allowTransfersOut, share.allowTransfersIn.
            Allowed comparisonOperator values: EQ, NEQ, GT, GTE, LT, LTE.
            Allowed valueType values: STRING, DECIMAL, BOOLEAN.
            Allowed actionType values: SEND_PUSH_NOTIFICATION, BLOCK_TRANSFER, AUTO_TRANSFER, ADD_TRANSACTION_TAG.
            For AUTO_TRANSFER actionConfig use: fromShareId, toShareId, amount (number or "ALL_EXCESS_ABOVE" with thresholdAmount).
            Use only share IDs from the provided member context.
            """;

        var userPrompt = $"""
            Member: {profile.Member.FirstName} {profile.Member.LastName} ({profile.Member.MemberId})

            Member shares:
            {JsonSerializer.Serialize(shareContext, JsonOptions)}

            Create a rule from this request:
            {request.Prompt}
            """;

        var response = await _geminiService.GenerateAsync(
            new GeminiChatRequest
            {
                Prompt = userPrompt,
                SystemInstruction = systemInstruction,
                ResponseAsJson = true,
            },
            cancellationToken);

        var rule = response.Json is { } json
            ? JsonSerializer.Deserialize<MoneyRule>(json.GetRawText(), JsonOptions)
                ?? throw new InvalidOperationException("Gemini returned invalid rule JSON.")
            : ParseRuleJson(response.Text ?? throw new InvalidOperationException("Gemini returned an empty response."));
        rule.MemberId = request.MemberId;
        rule.SourcePrompt = request.Prompt;

        if (string.IsNullOrWhiteSpace(rule.RuleId))
        {
            rule.RuleId = $"rule-{Guid.NewGuid():N}"[..12];
        }

        var saved = false;
        if (request.Save)
        {
            _ruleStore.AddRule(rule);
            saved = true;
        }

        return new GenerateRuleResponse
        {
            Rule = rule,
            Saved = saved
        };
    }

    private static MoneyRule ParseRuleJson(string text)
    {
        var json = ExtractJson(text);
        var rule = JsonSerializer.Deserialize<MoneyRule>(json, JsonOptions)
            ?? throw new InvalidOperationException("Gemini returned invalid rule JSON.");

        if (string.IsNullOrWhiteSpace(rule.RuleName))
        {
            throw new InvalidOperationException("Gemini rule JSON is missing ruleName.");
        }

        return rule;
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        var fenced = JsonFenceRegex().Match(trimmed);
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

        throw new InvalidOperationException("Gemini response did not contain JSON.");
    }

    [GeneratedRegex(@"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase)]
    private static partial Regex JsonFenceRegex();
}
