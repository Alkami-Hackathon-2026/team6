namespace AlkamiHackathon.Sandbox.Models;

public sealed class Account
{
    public string AccountId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Relationship { get; set; } = "Primary";
    public string Status { get; set; } = "Open";
    public DateOnly OpenDate { get; set; }
}
