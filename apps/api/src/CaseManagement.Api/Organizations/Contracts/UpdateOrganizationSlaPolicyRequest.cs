namespace CaseManagement.Api.Organizations.Contracts;

public sealed record UpdateOrganizationSlaPolicyRequest(
    int LowHours,
    int MediumHours,
    int HighHours);
