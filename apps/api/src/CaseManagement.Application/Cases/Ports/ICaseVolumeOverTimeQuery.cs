using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseVolumeOverTimeQuery
{
    Task<CaseVolumeOverTimeDto> GetAsync(
        Guid organizationId,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken = default);
}
