namespace CaseManagement.Application.Cases;
public sealed record GetMyCasesInput(
    Guid UserId,
    string? ClaimedOrganizationId,
    string? Cursor,
    int Limit,
    string? Search,
    string? Priority,
    string? Status,
    string? Sort,
    bool SortDescending,
    Guid? AssigneeUserId,
    bool OverdueOnly = false,
    bool BreachedOnly = false,
    bool UnassignedOnly = false,
    int? DueSoonWithinHours = null);