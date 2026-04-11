using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed record OrganizationMembershipResult(
    Guid Id,
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
