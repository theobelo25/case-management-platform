namespace CaseManagement.Api.Cases.Contracts;

public sealed record CaseStatusCountsResponse(
    int NewCount,
    int OpenCount,
    int PendingCount,
    int ResolvedCount,
    int ClosedCount);
