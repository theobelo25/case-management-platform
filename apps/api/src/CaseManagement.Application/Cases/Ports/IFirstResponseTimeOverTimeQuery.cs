using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface IFirstResponseTimeOverTimeQuery
{
    Task<FirstResponseTimeOverTimeDto> GetAsync(
        Guid organizationId,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken cancellationToken = default);
}
