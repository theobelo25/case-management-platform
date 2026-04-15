namespace CaseManagement.Api.Cases.Contracts;

public sealed record AddCaseCommentRequest(
    string Body,
    bool IsInternal);