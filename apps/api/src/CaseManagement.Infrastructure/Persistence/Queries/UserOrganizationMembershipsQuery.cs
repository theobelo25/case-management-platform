using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class UserOrganizationMembershipsQuery : IUserOrganizationMembershipsQuery
{
    private readonly CaseManagementDbContext _db;

    public UserOrganizationMembershipsQuery(
        CaseManagementDbContext db)
    {
        _db = db;
    }

    private sealed record OrgMembershipRow(
        Guid Id,
        string Name,
        OrganizationRole Role,
        DateTimeOffset CreatedAtUtc,
        bool IsArchived);

    public async Task<bool> IsUserMemberOfAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships
            .AsNoTracking()
            .AnyAsync(
                m => m.UserId == userId && m.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var rows = await BaseMembershipRowsForUser(userId).ToListAsync(cancellationToken);

        return rows
            .Select(x => new UserOrganizationSummaryDto(
                x.Id,
                x.Name,
                x.Role.ToString(),
                x.CreatedAtUtc,
                x.IsArchived))
            .ToArray();
    }

    public async Task<PagedList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId,
        int skip,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = BaseMembershipRowsForUser(userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .Skip(skip)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        var items = rows
            .Select(x => new UserOrganizationSummaryDto(
                x.Id,
                x.Name,
                x.Role.ToString(),
                x.CreatedAtUtc,
                x.IsArchived))
            .ToArray();
        
        var hasMore = skip + items.Length < totalCount;

        return new PagedList<UserOrganizationSummaryDto>(items,
            totalCount,
            skip,
            limit,
            hasMore);
    }

    private IQueryable<OrgMembershipRow>
        BaseMembershipRowsForUser(Guid userId)
    {
        return _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(
                _db.Organizations.AsNoTracking(),
                m => m.OrganizationId,
                o => o.Id,
                (m, o) => new { m, o })
            .OrderBy(x => x.o.Name)
            .Select(x => new OrgMembershipRow(x.o.Id, x.o.Name, x.m.Role, x.o.CreatedAtUtc, x.o.IsArchived));
    }
}