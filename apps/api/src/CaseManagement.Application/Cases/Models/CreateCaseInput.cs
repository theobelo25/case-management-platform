namespace CaseManagement.Application.Cases;

public sealed record CreateCaseInput(
    string Title,
    string InitialMessage,
    CasePriorityCode Priority,
    Guid? RequesterUserId,
    Guid CreatedByUserId);