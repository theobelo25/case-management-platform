using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface ICaseAnalyticsService
{
    Task<CaseVolumeOverTimeDto> GetCaseVolumeOverTimeAsync(
        GetCaseVolumeOverTimeInput input,
        CancellationToken cancellationToken = default);

    Task<FirstResponseTimeOverTimeDto> GetFirstResponseTimeOverTimeAsync(
        GetFirstResponseTimeOverTimeInput input,
        CancellationToken cancellationToken = default);

    Task<CaseStatusCountsDto> GetCaseStatusCountsAsync(
        GetCaseStatusSnapshotInput input,
        CancellationToken cancellationToken = default);
}
