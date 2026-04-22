using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class DueSoonRecipientResolver(
    IOrganizationPrivilegedUserIdsQuery privilegedUsers) : IDueSoonRecipientResolver
{
    public async Task<HashSet<Guid>> ResolveRecipientsAsync(
        Case @case,
        int windowMinutes,
        DueSoonProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        var recipients = new HashSet<Guid>();

        if (options.AssigneeWindows.Contains(windowMinutes) && @case.AssigneeUserId is { } assigneeId)
            recipients.Add(assigneeId);

        if (options.NotifyRequester
            && options.AssigneeWindows.Contains(windowMinutes)
            && @case.RequesterUserId is { } requesterId)
        {
            recipients.Add(requesterId);
        }

        if (options.PrivilegedWindows.Contains(windowMinutes))
        {
            var privilegedRecipientIds = await privilegedUsers.GetOwnerAndAdminUserIdsAsync(
                @case.OrganizationId,
                cancellationToken);
            foreach (var recipientId in privilegedRecipientIds)
                recipients.Add(recipientId);
        }

        return recipients;
    }
}
