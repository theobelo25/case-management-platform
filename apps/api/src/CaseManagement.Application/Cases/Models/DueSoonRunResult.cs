namespace CaseManagement.Application.Cases.Models;

public sealed class DueSoonRunResult
{
    public static readonly DueSoonRunResult Disabled = new();

    public int Scanned { get; set; }
    public int Breached { get; set; }
    public int Notified { get; set; }
    public int Skipped { get; set; }
    public int Deduped { get; set; }
    public int Errors { get; set; }
}
