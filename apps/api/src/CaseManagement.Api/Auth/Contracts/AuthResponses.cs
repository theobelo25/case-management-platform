using CaseManagement.Api.Organizations.Contracts;

namespace CaseManagement.Api.Auth.Contracts;
public sealed record AuthResponse(
    string AccessToken);

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid ActiveOrganizationId,
    IReadOnlyList<UserOrganizationResponse> Organizations);