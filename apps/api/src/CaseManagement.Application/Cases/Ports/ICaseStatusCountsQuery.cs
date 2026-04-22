using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseStatusCountsQuery
{
    Task<CaseStatusCountsDto> GetAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
