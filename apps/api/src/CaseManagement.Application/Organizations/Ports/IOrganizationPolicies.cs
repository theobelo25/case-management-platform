namespace CaseManagement.Application.Organizations.Ports;

public interface IOrganizationPolicies
{
    Task EnsureUserCanDelete(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task EnsureUserCanTransfer(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task EnsureUserCanArchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task EnsureUserCanUnarchive(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task EnsureUserCanAddMember(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
    
    Task EnsureRemoveMemberAllowed(
        Guid actorUserId,
        Guid memberIdToRemove,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
