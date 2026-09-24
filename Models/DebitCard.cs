namespace AlkamiHackathon.Sandbox.Models;

public sealed class DebitCard
{
    public string CardId { get; set; } = string.Empty;
    public string ShareId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string CardNumberMasked { get; set; } = string.Empty;
    public string LastFour { get; set; } = string.Empty;
    public string CardType { get; set; } = "Visa Debit";
    public string Status { get; set; } = "Active";
    public DateOnly ExpirationDate { get; set; }
    public string CardholderName { get; set; } = string.Empty;
}
