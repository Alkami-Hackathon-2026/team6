namespace AlkamiHackathon.Sandbox.Models;

public sealed class SimulateTransferResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Environment { get; set; } = "DEMO";
    public bool DryRun { get; set; } = true;
    public string? FromShareId { get; set; }
    public string? ToShareId { get; set; }
    public decimal Amount { get; set; }
    public decimal? FromBalanceAfter { get; set; }
    public decimal? ToBalanceAfter { get; set; }
}
