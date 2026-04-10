using CaseManagement.Application.Auth;
using CaseManagement.Application.Ports;
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
        var rows = await _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(
                _db.Organizations.AsNoTracking(),
                m => m.OrganizationId,
                o => o.Id,
                (m, o) => new { m, o })
            .OrderBy(x => x.o.Name)
            .Select(x => new { x.o.Id, x.o.Name, x.m.Role })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new UserOrganizationSummaryDto(x.Id, x.Name, x.Role.ToString()))
            .ToArray();
    }
}