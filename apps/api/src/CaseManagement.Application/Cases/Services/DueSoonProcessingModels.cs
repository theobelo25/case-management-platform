using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed record DueSoonProcessingOptions(
    bool Enabled,
    int BatchSize,
    int[] AssigneeWindows,
    int[] PrivilegedWindows,
    bool NotifyRequester,
    int[] AllWindows);

internal sealed record DueSoonSelectionResult(
    DateTimeOffset StartedAtUtc,
    DueSoonProcessingOptions Options,
    IReadOnlyList<Case> Candidates,
    int BreachedCount);
