namespace CaseManagement.Application.Cases.Models;

public sealed class FirstResponseTimeDayPointDto
{
    /// <summary>UTC calendar date (yyyy-MM-dd).</summary>
    public required string Date { get; init; }

    /// <summary>
    /// Average minutes from case creation to the first qualifying agent message for cases created on this day.
    /// Null when no case on that day had a qualifying first response.
    /// </summary>
    public double? AverageFirstResponseMinutes { get; init; }

    /// <summary>Cases created that day that had at least one qualifying first response.</summary>
    public int CasesWithFirstResponse { get; init; }
}
