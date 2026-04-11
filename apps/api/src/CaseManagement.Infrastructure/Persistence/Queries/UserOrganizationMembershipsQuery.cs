using CaseManagement.Application.Auth;
using CaseManagement.Application.Common;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class UserOrganizationMembershipsQuery : IUserOrganizationMembershipsQuery
{
    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _organizations;
    private readonly CaseManagementDbContext _db;

    public UserOrganizationMembershipsQuery(
        IUserRepository users,
        IOrganizationRepository organizations,
        CaseManagementDbContext db)
    {
        _users = users;
        _organizations = organizations;
        _db = db;
    }

    private sealed record OrgMembershipRow(Guid Id, string Name, OrganizationRole Role, DateTimeOffset CreatedAtUtc);

    public async Task<bool> IsUserMemberOfAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var membership = await _organizations.CheckUserMembership(
            userId, 
            organizationId, 
            cancellationToken);
            
        if (membership is null)
            return false;

        return true;
    }

    public async Task<IReadOnlyList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var rows = await BaseMembershipRowsForUser(userId).ToListAsync(cancellationToken);

        return rows
            .Select(x => new UserOrganizationSummaryDto(x.Id, x.Name, x.Role.ToString(), x.CreatedAtUtc))
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
            .Select(x => new UserOrganizationSummaryDto(x.Id, x.Name, x.Role.ToString(), x.CreatedAtUtc))
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
            .Select(x => new OrgMembershipRow(x.o.Id, x.o.Name, x.m.Role, x.o.CreatedAtUtc));
    }
}