namespace AlkamiHackathon.Sandbox.Models;

public sealed class Member
{
    public string MemberId { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Status { get; set; } = "Active";
    public DateOnly MemberSince { get; set; }
}
