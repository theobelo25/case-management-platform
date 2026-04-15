using CaseManagement.Application.Ports;
using CaseManagement.Application.Users;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases;

public sealed class CasesService(
    IUsersService users,
    ICaseRepository cases,
    IUnitOfWork unitOfWork
) : ICasesService
{
    public async Task<CaseDetailDto> Create(
        CreateCaseInput input, 
        CancellationToken cancellationToken = default)
    {
        string? requesterName = null;

        if (input.RequesterUserId is {} requesterUserID)
        {
            requesterName = await users.GetName(
                requesterUserID, 
                cancellationToken);
        }

        var priority = input.Priority switch
        {
            CasePriorityCode.LOW => CasePriority.Low,
            CasePriorityCode.MEDIUM => CasePriority.Medium,
            CasePriorityCode.HIGH => CasePriority.High,
            _ => throw new ArgumentOutOfRangeException(nameof(input.Priority), input.Priority, null),
        };

        var newCase = Case.Create(
            input.Title,
            input.CreatedByUserId,
            input.InitialMessage,
            priority,
            input.RequesterUserId,
            requesterName);
        
        cases.Add(newCase);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var timeline = newCase.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new CaseTimelineItemDto(
                Type: "message",
                Id: m.Id,
                CreatedAtUtc: m.CreatedAtUtc,
                AuthorUserId: m.AuthorUserId,
                Body: m.Body,
                IsInternal: m.IsInternal,
                IsInitial: m.IsInitial,
                EventType: null,
                Metadata: null))
            .ToList();

        return new CaseDetailDto(
            Id: newCase.Id,
            Title: newCase.Title,
            Status: newCase.Status.ToString(),
            Priority: ToApiString(newCase.Priority),
            RequesterUserId: newCase.RequesterUserId,
            RequesterName: newCase.RequesterName,
            AssigneeUserId: newCase.AssigneeUserId,
            CreatedByUserId: newCase.CreatedByUserId,
            CreatedAtUtc: newCase.CreatedAtUtc,
            UpdatedAtUtc: newCase.UpdatedAtUtc,
            Timeline: timeline);
    }

    private static CasePriorityCode ToCode(CasePriority priority) =>
        priority switch
        {
            CasePriority.Low => CasePriorityCode.LOW,
            CasePriority.Medium => CasePriorityCode.MEDIUM,
            CasePriority.High => CasePriorityCode.HIGH,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };

    private static string ToApiString(CasePriority priority) =>
        ToCode(priority).ToString();
}