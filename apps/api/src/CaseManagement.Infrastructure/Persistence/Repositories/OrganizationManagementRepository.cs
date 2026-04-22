using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Repositories;

public sealed class OrganizationManagementRepository(
    CaseManagementDbContext db) : IOrganizationManagementRepository
{
    public Task<Organization> Create(
        string name,
        CancellationToken cancellationToken = default)
    {
        var organization = Organization.Create(name);
        db.Organizations.Add(organization);

        return Task.FromResult(organization);
    }

    public async Task<Organization> Archive(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        organization.Archive();
        return organization;
    }

    public async Task<Organization> Unarchive(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        organization.Unarchive();
        return organization;
    }

    public async Task<bool> Delete(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization is null)
            return false;

        db.Organizations.Remove(organization);
        return true;
    }

    public async Task<Organization> UpdateSlaPolicy(
        Guid organizationId,
        int lowHours,
        int mediumHours,
        int highHours,
        CancellationToken cancellationToken = default)
    {
        var organization = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        organization.UpdateSlaPolicy(lowHours, mediumHours, highHours);
        return organization;
    }
}
