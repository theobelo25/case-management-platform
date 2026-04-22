namespace CaseManagement.Api.Cases.Contracts;

public sealed record UpdateCaseRequest(
    string Status,
    string Priority);
