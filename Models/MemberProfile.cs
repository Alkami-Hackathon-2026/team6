namespace AlkamiHackathon.Sandbox.Models;

public sealed class MemberProfile
{
    public Member Member { get; set; } = new();
    public IReadOnlyList<Account> Accounts { get; set; } = Array.Empty<Account>();
    public IReadOnlyList<ShareDetail> Shares { get; set; } = Array.Empty<ShareDetail>();
}
