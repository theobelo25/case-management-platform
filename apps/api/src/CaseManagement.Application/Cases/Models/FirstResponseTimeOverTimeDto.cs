namespace CaseManagement.Application.Cases.Models;

public sealed class FirstResponseTimeOverTimeDto
{
    public required IReadOnlyList<FirstResponseTimeDayPointDto> Series { get; init; }
}
