using CaseManagement.Application.Organizations;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(
    CaseManagementDbContext db) : IOrganizationRepository
{
    public async Task<OrganizationResult> Create(
        string name, 
        CancellationToken cancellationToken = default)
    {
        var organization = Organization.Create(name);
        db.Organizations.Add(organization);
        
        return new OrganizationResult(
            organization.Id,
            organization.Name,
            organization.CreatedAtUtc);
    }

    public async Task<OrganizationMembershipResult> IssueMembership(
        Guid userId,
        Guid organizationId,
        OrganizationRole role,
        CancellationToken cancellationToken = default)
    {
        var membership = new OrganizationMembership(
            organizationId,
            userId, 
            role);

        db.OrganizationMemberships.Add(membership);

        return new OrganizationMembershipResult(
            membership.Id,
            membership.UserId,
            membership.OrganizationId,
            membership.Role,
            membership.CreatedAtUtc,
            membership.UpdatedAtUtc
        );
    }
}