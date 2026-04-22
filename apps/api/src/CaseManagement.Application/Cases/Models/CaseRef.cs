namespace CaseManagement.Application.Cases.Models;

public sealed record CaseRef(Guid OrganizationId, Guid CaseId, string CaseTitle);
