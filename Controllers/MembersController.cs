using AlkamiHackathon.Sandbox.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersController : ControllerBase
{
    private readonly ISandboxDataStore _dataStore;

    public MembersController(ISandboxDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult GetAll() => Ok(_dataStore.GetMembers());

    [HttpGet("{memberId}")]
    public ActionResult GetById(string memberId)
    {
        var member = _dataStore.GetMemberById(memberId);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet("by-number/{memberNumber}")]
    public ActionResult GetByNumber(string memberNumber)
    {
        var member = _dataStore.GetMemberByNumber(memberNumber);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet("{memberId}/profile")]
    public ActionResult GetProfile(string memberId)
    {
        var profile = _dataStore.GetMemberProfile(memberId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{memberId}/accounts")]
    public ActionResult GetAccounts(string memberId)
    {
        if (_dataStore.GetMemberById(memberId) is null)
        {
            return NotFound();
        }

        return Ok(_dataStore.GetAccountsByMemberId(memberId));
    }

    [HttpGet("{memberId}/shares")]
    public ActionResult GetShares(string memberId)
    {
        if (_dataStore.GetMemberById(memberId) is null)
        {
            return NotFound();
        }

        return Ok(_dataStore.GetSharesByMemberId(memberId));
    }

    [HttpGet("{memberId}/debit-cards")]
    public ActionResult GetDebitCards(string memberId)
    {
        if (_dataStore.GetMemberById(memberId) is null)
        {
            return NotFound();
        }

        return Ok(_dataStore.GetDebitCardsByMemberId(memberId));
    }
}
