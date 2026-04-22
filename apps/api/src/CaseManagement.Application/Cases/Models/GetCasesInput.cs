namespace CaseManagement.Application.Cases;

public sealed record GetCasesInput(
    Guid OrganizationId,
    string? Cursor,
    int Limit,
    CaseListFilters Filters,
    CaseListSort Sort);

public sealed record CaseListFilters(
    string? Search,
    string? Priority,
    string? Status,
    Guid? AssigneeUserId,
    bool OverdueOnly = false,
    bool BreachedOnly = false,
    bool UnassignedOnly = false,
    int? DueSoonWithinHours = null);

public sealed record CaseListSort(
    string? Field,
    bool Descending = false);
