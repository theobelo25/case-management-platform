namespace CaseManagement.Application.Cases.Services;

internal interface IDueSoonCaseSelectionService
{
    Task<DueSoonSelectionResult> SelectAsync(CancellationToken cancellationToken = default);
}
