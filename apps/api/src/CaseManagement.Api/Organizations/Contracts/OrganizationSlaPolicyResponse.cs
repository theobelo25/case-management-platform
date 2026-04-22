namespace CaseManagement.Api.Organizations.Contracts;

public sealed record OrganizationSlaPolicyResponse(
    int LowHours,
    int MediumHours,
    int HighHours);
