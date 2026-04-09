using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed record OrganizationMembershipResult(
    Guid Id,
    Guid userId,
    Guid organizationId,
    OrganizationRole role,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
