namespace CaseManagement.Application.Cases.Models;

public sealed record CaseStatusCountsDto(
    int NewCount,
    int OpenCount,
    int PendingCount,
    int ResolvedCount,
    int ClosedCount);
