namespace AlkamiHackathon.Sandbox.Models;

public sealed class Share
{
    public string ShareId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string ShareType { get; set; } = string.Empty;
    public string ShareCode { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = "Open";
    public bool AllowTransfersIn { get; set; } = true;
    public bool AllowTransfersOut { get; set; } = true;
}
