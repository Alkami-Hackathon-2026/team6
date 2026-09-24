using System.Text.Json;
using System.Text.Json.Serialization;
using AlkamiHackathon.Sandbox.Models;

namespace AlkamiHackathon.Sandbox.Services;

public sealed class JsonSandboxDataStore : ISandboxDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<Member> _members;
    private readonly List<Account> _accounts;
    private readonly List<Share> _shares;
    private readonly List<DebitCard> _debitCards;

    public JsonSandboxDataStore(IWebHostEnvironment environment)
    {
        var dataPath = Path.Combine(environment.ContentRootPath, "Data");

        _members = Load<List<Member>>(Path.Combine(dataPath, "members.json"));
        _accounts = Load<List<Account>>(Path.Combine(dataPath, "accounts.json"));
        _shares = Load<List<Share>>(Path.Combine(dataPath, "shares.json"));
        _debitCards = Load<List<DebitCard>>(Path.Combine(dataPath, "debit-cards.json"));
    }

    public IReadOnlyList<Member> GetMembers() => _members;

    public Member? GetMemberById(string memberId) =>
        _members.FirstOrDefault(m => m.MemberId.Equals(memberId, StringComparison.OrdinalIgnoreCase));

    public Member? GetMemberByNumber(string memberNumber) =>
        _members.FirstOrDefault(m => m.MemberNumber.Equals(memberNumber, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Account> GetAccounts() => _accounts;

    public IReadOnlyList<Account> GetAccountsByMemberId(string memberId) =>
        _accounts.Where(a => a.MemberId.Equals(memberId, StringComparison.OrdinalIgnoreCase)).ToList();

    public Account? GetAccountById(string accountId) =>
        _accounts.FirstOrDefault(a => a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Share> GetShares() => _shares;

    public IReadOnlyList<Share> GetSharesByMemberId(string memberId) =>
        _shares.Where(s => s.MemberId.Equals(memberId, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<Share> GetSharesByAccountId(string accountId) =>
        _shares.Where(s => s.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase)).ToList();

    public Share? GetShareById(string shareId) =>
        _shares.FirstOrDefault(s => s.ShareId.Equals(shareId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<DebitCard> GetDebitCards() => _debitCards;

    public IReadOnlyList<DebitCard> GetDebitCardsByShareId(string shareId) =>
        _debitCards.Where(c => c.ShareId.Equals(shareId, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<DebitCard> GetDebitCardsByMemberId(string memberId) =>
        _debitCards.Where(c => c.MemberId.Equals(memberId, StringComparison.OrdinalIgnoreCase)).ToList();

    public MemberProfile? GetMemberProfile(string memberId)
    {
        var member = GetMemberById(memberId);
        if (member is null)
        {
            return null;
        }

        var accounts = GetAccountsByMemberId(memberId);
        var shareDetails = GetSharesByMemberId(memberId)
            .Select(share => new ShareDetail
            {
                Share = share,
                DebitCards = GetDebitCardsByShareId(share.ShareId)
            })
            .ToList();

        return new MemberProfile
        {
            Member = member,
            Accounts = accounts,
            Shares = shareDetails
        };
    }

    public SimulateTransferResponse SimulateTransfer(SimulateTransferRequest request)
    {
        if (request.Amount <= 0)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = "Amount must be greater than zero.",
                Amount = request.Amount
            };
        }

        var fromShare = GetShareById(request.FromShareId);
        if (fromShare is null)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = $"From share '{request.FromShareId}' was not found.",
                Amount = request.Amount
            };
        }

        var toShare = GetShareById(request.ToShareId);
        if (toShare is null)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = $"To share '{request.ToShareId}' was not found.",
                Amount = request.Amount
            };
        }

        if (!fromShare.AllowTransfersOut)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = $"Share '{fromShare.Nickname}' does not allow transfers out.",
                FromShareId = fromShare.ShareId,
                ToShareId = toShare.ShareId,
                Amount = request.Amount
            };
        }

        if (!toShare.AllowTransfersIn)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = $"Share '{toShare.Nickname}' does not allow transfers in.",
                FromShareId = fromShare.ShareId,
                ToShareId = toShare.ShareId,
                Amount = request.Amount
            };
        }

        if (fromShare.AvailableBalance < request.Amount)
        {
            return new SimulateTransferResponse
            {
                Success = false,
                Message = $"Insufficient funds in '{fromShare.Nickname}'. Available: {fromShare.AvailableBalance:C}.",
                FromShareId = fromShare.ShareId,
                ToShareId = toShare.ShareId,
                Amount = request.Amount
            };
        }

        return new SimulateTransferResponse
        {
            Success = true,
            Message = $"Demo transfer of {request.Amount:C} from '{fromShare.Nickname}' to '{toShare.Nickname}' would succeed.",
            FromShareId = fromShare.ShareId,
            ToShareId = toShare.ShareId,
            Amount = request.Amount,
            FromBalanceAfter = fromShare.Balance - request.Amount,
            ToBalanceAfter = toShare.Balance + request.Amount
        };
    }

    private static T Load<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Unable to load sandbox data from '{path}'.");
    }
}
