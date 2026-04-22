namespace CaseManagement.Application.Cases;

public sealed record AddCaseCommentInput(
    Guid UserId,
    Guid CaseId,
    string? ClaimedOrganizationId,
    string Body,
    bool IsInternal);
