using CaseManagement.Application.Ports;
using CaseManagement.Application.Users;
using CaseManagement.Application.Exceptions;
using CaseManagement.Domain.Entities;
using CaseManagement.Application.Common;

namespace CaseManagement.Application.Cases;

public sealed class CasesService(
    IUsersService users,
    IUserRepository userRepository,
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

        var createdByUser = await userRepository.GetByIdAsync(
            input.CreatedByUserId,
            cancellationToken)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);

        var organizationId = createdByUser.ActiveOrganizationId
            ?? throw new NotFoundException("No active organization", AppErrorCodes.NoActiveOrganization);

        var newCase = Case.Create(
            organizationId,
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

    public async Task<CursorPage<CaseListItemDto>> GetCasesAsync(
        GetCasesInput input,
        CancellationToken cancellationToken = default)
    {
        var casesPage = await cases.GetCases(
            input,
            cancellationToken);

        var items = casesPage.Items
            .Select(c => new CaseListItemDto(
                Id: c.Id,
                Title: c.Title,
                Status: c.Status.ToString(),
                Priority: ToApiString(c.Priority),
                RequesterUserId: c.RequesterUserId,
                RequesterName: c.RequesterName,
                AssigneeUserId: c.AssigneeUserId,
                CreatedByUserId: c.CreatedByUserId,
                CreatedAtUtc: c.CreatedAtUtc,
                UpdatedAtUtc: c.UpdatedAtUtc))
            .ToArray();

        return new CursorPage<CaseListItemDto>(
            items,
            casesPage.NextCursor,
            casesPage.Limit);
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