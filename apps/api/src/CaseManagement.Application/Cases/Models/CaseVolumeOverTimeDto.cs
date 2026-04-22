namespace CaseManagement.Application.Cases.Models;

public sealed class CaseVolumeOverTimeDto
{
    public required IReadOnlyList<CaseVolumeDayPointDto> Series { get; init; }
}
