using AlkamiHackathon.Sandbox.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlkamiHackathon.Sandbox.Controllers;

[ApiController]
[Route("api/shares")]
public sealed class SharesController : ControllerBase
{
    private readonly ISandboxDataStore _dataStore;

    public SharesController(ISandboxDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public ActionResult GetAll() => Ok(_dataStore.GetShares());

    [HttpGet("{shareId}")]
    public ActionResult GetById(string shareId)
    {
        var share = _dataStore.GetShareById(shareId);
        return share is null ? NotFound() : Ok(share);
    }

    [HttpGet("{shareId}/detail")]
    public ActionResult GetDetail(string shareId)
    {
        var share = _dataStore.GetShareById(shareId);
        if (share is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            share,
            debitCards = _dataStore.GetDebitCardsByShareId(shareId)
        });
    }

    [HttpGet("{shareId}/debit-cards")]
    public ActionResult GetDebitCards(string shareId)
    {
        if (_dataStore.GetShareById(shareId) is null)
        {
            return NotFound();
        }

        return Ok(_dataStore.GetDebitCardsByShareId(shareId));
    }
}
