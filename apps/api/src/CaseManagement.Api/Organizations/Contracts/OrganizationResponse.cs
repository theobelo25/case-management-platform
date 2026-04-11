using CaseManagement.Domain.Entities;

namespace CaseManagement.Api.Organizations.Contracts;

public sealed record OrganizationResponse(
    Guid Id, 
    string Name,
    DateTimeOffset CreatedAtUtc);
