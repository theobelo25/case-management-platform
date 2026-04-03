namespace CaseManagement.Application.Auth;

public sealed record MeResponse(
    Guid UserId,
    string Email,
    string FullName
);
