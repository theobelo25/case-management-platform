namespace CaseManagement.Application.Users;

public sealed record UserSearchResult(
    Guid UserId,
    string FullName,
    string Email);