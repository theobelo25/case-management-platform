namespace CaseManagement.Application.Cases.Models;

public sealed class CaseVolumeDayPointDto
{
    /// <summary>UTC calendar date (yyyy-MM-dd).</summary>
    public required string Date { get; init; }

    public int CasesCreated { get; init; }

    public int CasesResolved { get; init; }

    public int CasesReopened { get; init; }
}
