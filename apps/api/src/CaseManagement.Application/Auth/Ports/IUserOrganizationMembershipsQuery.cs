using CaseManagement.Application.Common;

namespace CaseManagement.Application.Auth;

public interface IUserOrganizationMembershipsQuery
{
    Task<IReadOnlyList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<PagedList<UserOrganizationSummaryDto>> ListForUserAsync(
        Guid userId,
        int skip,
        int limit,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsUserMemberOfAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}