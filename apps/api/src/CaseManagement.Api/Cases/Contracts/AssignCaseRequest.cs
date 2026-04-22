namespace CaseManagement.Api.Cases.Contracts;

public sealed record AssignCaseRequest(
    Guid? AssigneeUserId);
