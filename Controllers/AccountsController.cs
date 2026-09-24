using AlkamiHackathon.Sandbox.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly ISandboxDataStore _dataStore;

    public AccountsController(ISandboxDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult GetAll() => Ok(_dataStore.GetAccounts());

    [HttpGet("{accountId}")]
    public ActionResult GetById(string accountId)
    {
        var account = _dataStore.GetAccountById(accountId);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("{accountId}/shares")]
    public ActionResult GetShares(string accountId)
    {
        if (_dataStore.GetAccountById(accountId) is null)
        {
            return NotFound();
        }

        return Ok(_dataStore.GetSharesByAccountId(accountId));
    }
}
