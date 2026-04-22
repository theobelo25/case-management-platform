namespace CaseManagement.Application.Cases.Models;

public sealed class DueSoonSchedulerOptions
{
    public const string SectionName = "DueSoonScheduler";

    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 300;
    public int BatchSize { get; set; } = 200;
    public int[] AssigneeWindowsMinutes { get; set; } = [240, 60, 15];
    public int[] PrivilegedWindowsMinutes { get; set; } = [15];
    public bool NotifyRequester { get; set; } = false;
}
