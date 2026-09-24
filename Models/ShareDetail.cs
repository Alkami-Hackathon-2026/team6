namespace AlkamiHackathon.Sandbox.Models;

public sealed class ShareDetail
{
    public Share Share { get; set; } = new();
    public IReadOnlyList<DebitCard> DebitCards { get; set; } = Array.Empty<DebitCard>();
}
