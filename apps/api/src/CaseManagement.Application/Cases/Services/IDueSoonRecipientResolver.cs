using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal interface IDueSoonRecipientResolver
{
    Task<HashSet<Guid>> ResolveRecipientsAsync(
        Case @case,
        int windowMinutes,
        DueSoonProcessingOptions options,
        CancellationToken cancellationToken = default);
}
