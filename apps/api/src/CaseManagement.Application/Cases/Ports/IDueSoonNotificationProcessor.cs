using CaseManagement.Application.Cases.Models;

namespace CaseManagement.Application.Cases.Ports;

public interface IDueSoonNotificationProcessor
{
    Task<DueSoonRunResult> RunOnceAsync(CancellationToken cancellationToken = default);
}
