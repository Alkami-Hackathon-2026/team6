using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public interface ISandboxDataStore
{
    IReadOnlyList<Member> GetMembers();
    Member? GetMemberById(string memberId);
    Member? GetMemberByNumber(string memberNumber);

    IReadOnlyList<Account> GetAccounts();
    IReadOnlyList<Account> GetAccountsByMemberId(string memberId);
    Account? GetAccountById(string accountId);

    IReadOnlyList<Share> GetShares();
    IReadOnlyList<Share> GetSharesByMemberId(string memberId);
    IReadOnlyList<Share> GetSharesByAccountId(string accountId);
    Share? GetShareById(string shareId);

    IReadOnlyList<DebitCard> GetDebitCards();
    IReadOnlyList<DebitCard> GetDebitCardsByShareId(string shareId);
    IReadOnlyList<DebitCard> GetDebitCardsByMemberId(string memberId);

    MemberProfile? GetMemberProfile(string memberId);
    SimulateTransferResponse SimulateTransfer(SimulateTransferRequest request);
}
