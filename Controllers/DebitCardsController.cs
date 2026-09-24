using AlkamiHackathon.Sandbox.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/debit-cards")]
public sealed class DebitCardsController : ControllerBase
{
    private readonly ISandboxDataStore _dataStore;

    public DebitCardsController(ISandboxDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult GetAll() => Ok(_dataStore.GetDebitCards());
}
