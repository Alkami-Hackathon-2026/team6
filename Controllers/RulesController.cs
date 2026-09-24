using AlkamiHackathon.Sandbox.Models;
using AlkamiHackathon.Sandbox.Services;
using Google.GenAI;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/rules")]
public sealed class RulesController : ControllerBase
{
    private readonly IRuleStore _ruleStore;
    private readonly IGeminiRuleService _geminiRuleService;
    private readonly IGeminiService _geminiService;

    public RulesController(
        IRuleStore ruleStore,
        IGeminiRuleService geminiRuleService,
        IGeminiService geminiService)
    {
        _ruleStore = ruleStore;
        _geminiRuleService = geminiRuleService;
        _geminiService = geminiService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<MoneyRule>> GetRules() => Ok(_ruleStore.GetRules());

    [HttpGet("member/{memberId}")]
    public ActionResult<IReadOnlyList<MoneyRule>> GetRulesByMember(string memberId) =>
        Ok(_ruleStore.GetRulesByMemberId(memberId));

    [HttpGet("{ruleId}")]
    public ActionResult<MoneyRule> GetRule(string ruleId)
    {
        var rule = _ruleStore.GetRuleById(ruleId);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    public ActionResult<MoneyRule> CreateRule([FromBody] MoneyRule rule)
    {
        try
        {
            rule.MemberId = string.IsNullOrWhiteSpace(rule.MemberId)
                ? throw new ArgumentException("MemberId is required.")
                : rule.MemberId;

            return CreatedAtAction(nameof(GetRule), new { ruleId = rule.RuleId }, _ruleStore.AddRule(rule));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{ruleId}")]
    public ActionResult<MoneyRule> UpdateRule(string ruleId, [FromBody] MoneyRule rule)
    {
        if (!ruleId.Equals(rule.RuleId, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "RuleId in URL must match the body." });
        }

        try
        {
            return Ok(_ruleStore.UpdateRule(rule));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{ruleId}")]
    public ActionResult DeleteRule(string ruleId)
    {
        return _ruleStore.DeleteRule(ruleId) ? NoContent() : NotFound();
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateRuleResponse>> GenerateRule(
        [FromBody] GenerateRuleRequest request,
        CancellationToken cancellationToken)
    {
        if (!_geminiService.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Gemini is not configured.",
                hint = "Set Gemini:ApiKey in appsettings.json."
            });
        }

        try
        {
            var response = await _geminiRuleService.GenerateRuleAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ClientError ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ServerError ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
    }
}
