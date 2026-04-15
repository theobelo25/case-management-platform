namespace CaseManagement.Api.Users.Contracts;

public sealed record UserSearchResponse(Guid UserId, string FullName, string Email);
