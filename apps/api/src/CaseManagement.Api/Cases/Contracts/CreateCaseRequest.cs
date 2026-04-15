using CaseManagement.Application.Cases;

namespace CaseManagement.Api.Cases.Contracts;

public sealed record CreateCaseRequest(
    string Title,
    string InitialMessage,
    CasePriorityCode Priority,
    Guid? RequesterUserId);