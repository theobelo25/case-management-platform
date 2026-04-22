namespace CaseManagement.Api.Cases.Contracts;

public sealed record CaseVolumeDayPointResponse(
    string Date,
    int CasesCreated,
    int CasesResolved,
    int CasesReopened);

public sealed record CaseVolumeOverTimeResponse(IReadOnlyList<CaseVolumeDayPointResponse> Series);
