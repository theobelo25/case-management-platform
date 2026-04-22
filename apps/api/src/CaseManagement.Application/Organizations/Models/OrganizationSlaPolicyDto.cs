namespace CaseManagement.Application.Organizations;

public sealed record OrganizationSlaPolicyDto(
    int LowHours,
    int MediumHours,
    int HighHours);
