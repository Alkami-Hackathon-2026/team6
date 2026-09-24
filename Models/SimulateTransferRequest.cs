namespace AlkamiHackathon.Sandbox.Models;

public sealed class SimulateTransferRequest
{
    public string FromShareId { get; set; } = string.Empty;
    public string ToShareId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
}
