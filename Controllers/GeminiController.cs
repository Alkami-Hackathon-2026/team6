using AlkamiHackathon.Sandbox.Models;
using AlkamiHackathon.Sandbox.Services;
using Google.GenAI;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/gemini")]
[Produces("application/json")]
public sealed class GeminiController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public GeminiController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpGet("status")]
    public ActionResult Status() => Ok(new
    {
        configured = _geminiService.IsConfigured,
        model = _geminiService.Model,
        message = _geminiService.IsConfigured
            ? "Gemini free-tier API is ready for testing."
            : "Set Gemini:ApiKey in user secrets or GEMINI_API_KEY in the environment."
    });

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] GeminiChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!_geminiService.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Gemini is not configured.",
                hint = "Run: dotnet user-secrets set \"Gemini:ApiKey\" \"YOUR_KEY\" from Google AI Studio."
            });
        }

        try
        {
            var response = await _geminiService.GenerateAsync(request, cancellationToken);
            var responseAsJson = request.ResponseAsJson ?? true;

            if (responseAsJson && response.Json is { } json)
            {
                return new JsonResult(new
                {
                    model = response.Model,
                    data = json
                });
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
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
