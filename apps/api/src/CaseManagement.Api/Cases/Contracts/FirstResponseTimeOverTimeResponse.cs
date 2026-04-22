namespace CaseManagement.Api.Cases.Contracts;

public sealed record FirstResponseTimeDayPointResponse(
    string Date,
    double? AverageFirstResponseMinutes,
    int CasesWithFirstResponse);

public sealed record FirstResponseTimeOverTimeResponse(
    IReadOnlyList<FirstResponseTimeDayPointResponse> Series);
