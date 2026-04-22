using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Exceptions;

namespace CaseManagement.Application.Cases.Services;

internal sealed class CaseAnalyticsService(
    CaseAccessResolver accessResolver,
    ICaseVolumeOverTimeQuery caseVolumeOverTimeQuery,
    IFirstResponseTimeOverTimeQuery firstResponseTimeOverTimeQuery,
    ICaseStatusCountsQuery caseStatusCountsQuery) : ICaseAnalyticsService
{
    public async Task<CaseVolumeOverTimeDto> GetCaseVolumeOverTimeAsync(
        GetCaseVolumeOverTimeInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.Days is < 1 or > 90)
            throw new BadRequestArgumentException("Days must be between 1 and 90.");

        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var toInclusive = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromInclusive = toInclusive.AddDays(-(input.Days - 1));

        return await caseVolumeOverTimeQuery.GetAsync(
            activeOrganizationId,
            fromInclusive,
            toInclusive,
            cancellationToken);
    }

    public async Task<FirstResponseTimeOverTimeDto> GetFirstResponseTimeOverTimeAsync(
        GetFirstResponseTimeOverTimeInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.Days is < 1 or > 90)
            throw new BadRequestArgumentException("Days must be between 1 and 90.");

        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        var toInclusive = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromInclusive = toInclusive.AddDays(-(input.Days - 1));

        return await firstResponseTimeOverTimeQuery.GetAsync(
            activeOrganizationId,
            fromInclusive,
            toInclusive,
            cancellationToken);
    }

    public async Task<CaseStatusCountsDto> GetCaseStatusCountsAsync(
        GetCaseStatusSnapshotInput input,
        CancellationToken cancellationToken = default)
    {
        var activeOrganizationId = await accessResolver.ResolveActiveOrganizationIdAsync(
            input.UserId,
            input.ClaimedOrganizationId,
            cancellationToken);

        return await caseStatusCountsQuery.GetAsync(activeOrganizationId, cancellationToken);
    }
}
