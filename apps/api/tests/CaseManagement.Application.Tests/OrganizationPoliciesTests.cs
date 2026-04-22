using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;
using FluentAssertions;

namespace CaseManagement.Application.Tests;

public sealed class OrganizationPoliciesTests
{
    [Fact]
    public async Task EnsureUserCanDelete_throws_Forbidden_when_user_is_admin()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repo = new FakeOrgReadRepository((userId, orgId, OrganizationRole.Admin));
        var policies = new OrganizationPolicies(repo);

        var act = () => policies.EnsureUserCanDelete(userId, orgId, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*Owner*");
    }

    [Fact]
    public async Task EnsureUserCanDelete_completes_when_user_is_owner()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repo = new FakeOrgReadRepository((userId, orgId, OrganizationRole.Owner));
        var policies = new OrganizationPolicies(repo);

        await policies.EnsureUserCanDelete(userId, orgId, CancellationToken.None);
    }

    [Fact]
    public async Task EnsureUserCanDelete_throws_NotFound_when_membership_missing()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repo = new FakeOrgReadRepository();
        var policies = new OrganizationPolicies(repo);

        var act = () => policies.EnsureUserCanDelete(userId, orgId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureRemoveMemberAllowed_throws_BadRequest_when_removing_owner()
    {
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var repo = new FakeOrgReadRepository(
            (ownerId, orgId, OrganizationRole.Owner),
            (actorId, orgId, OrganizationRole.Owner));
        var policies = new OrganizationPolicies(repo);

        var act = () => policies.EnsureRemoveMemberAllowed(actorId, ownerId, orgId, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestArgumentException>();
    }

    private sealed class FakeOrgReadRepository : IOrganizationReadRepository
    {
        private readonly Dictionary<(Guid UserId, Guid OrgId), OrganizationRole?> _memberships;

        public FakeOrgReadRepository(params (Guid userId, Guid orgId, OrganizationRole? role)[] rows)
        {
            _memberships = [];
            foreach (var (userId, orgId, role) in rows)
                _memberships[(userId, orgId)] = role;
        }

        public Task<Organization?> GetById(Guid organizationId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Organization?>(null);

        public Task<OrganizationRole?> CheckUserMembership(
            Guid userId,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            if (_memberships.TryGetValue((userId, organizationId), out var role))
                return Task.FromResult(role);
            return Task.FromResult<OrganizationRole?>(null);
        }

        public Task<IReadOnlyList<Guid>> GetOwnerAndAdminUserIds(
            Guid organizationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }
}
