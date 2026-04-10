namespace CaseManagement.Application.Auth;

public interface IUserOrganizationMembershipsQuery
{
    Task<IReadOnlyList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsUserMemberOfAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}