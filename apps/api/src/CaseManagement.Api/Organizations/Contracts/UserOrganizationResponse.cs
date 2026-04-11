namespace CaseManagement.Api.Organizations.Contracts;

public sealed record UserOrganizationResponse(
    Guid Id,
    string Name,
    string Role
);