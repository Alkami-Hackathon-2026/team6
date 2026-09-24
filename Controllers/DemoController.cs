using AlkamiHackathon.Sandbox.Models;
using AlkamiHackathon.Sandbox.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly ISandboxDataStore _dataStore;

    public DemoController(ISandboxDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet("health")]
    public ActionResult Health() => Ok(new
    {
        status = "ok",
        environment = "local-sandbox",
        message = "Alkami hackathon sandbox is running."
    });

    [HttpPost("transfers/simulate")]
    public ActionResult SimulateTransfer([FromBody] SimulateTransferRequest request)
    {
        var result = _dataStore.SimulateTransfer(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
