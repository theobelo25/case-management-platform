namespace CaseManagement.Api.Organizations.Contracts;

public sealed record OrganizationMemberResponse(
    Guid Id,
    string Name,
    string Role,
    string Email,
    DateTimeOffset JoinedAtUtc);