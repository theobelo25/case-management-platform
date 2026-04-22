using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Cases.Models;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Application.Cases.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Cases.Services;

internal sealed class CaseCreationService(
    IUserDisplayNameLookup userDisplayNames,
    IUserRepository userRepository,
    IOrganizationReadRepository organizations,
    ICaseRepository cases,
    IUnitOfWork unitOfWork) : ICaseCreationService
{
    public async Task<CaseDetailDto> Create(
        CreateCaseInput input,
        CancellationToken cancellationToken = default)
    {
        string? requesterName = null;
        if (input.RequesterUserId is { } requesterUserId)
        {
            requesterName = await userDisplayNames.GetName(requesterUserId, cancellationToken);
        }

        var priority = CaseStatusPriorityMapper.ToDomainPriority(input.Priority);

        var createdByUser = await userRepository.GetByIdAsync(input.CreatedByUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);

        var organizationId = createdByUser.ActiveOrganizationId
            ?? throw new NotFoundException("No active organization", AppErrorCodes.NoActiveOrganization);

        var organizationEntity = await organizations.GetById(organizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");
        var slaPolicy = organizationEntity.GetSlaDurationPolicy();

        var newCase = Case.Create(
            organizationId,
            input.Title,
            input.CreatedByUserId,
            input.InitialMessage,
            slaPolicy,
            priority,
            input.RequesterUserId,
            requesterName);

        cases.Add(newCase);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createMessageAuthorIds = newCase.Messages.Select(m => m.AuthorUserId).Distinct().ToList();
        var createAuthorNames = await userDisplayNames.GetDisplayNamesByIdsAsync(createMessageAuthorIds, cancellationToken);

        var timeline = newCase.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new CaseTimelineItemDto(
                Type: "message",
                Id: m.Id,
                CreatedAtUtc: m.CreatedAtUtc,
                AuthorUserId: m.AuthorUserId,
                AuthorDisplayName: CaseServiceMappings.ResolveAuthorDisplayName(m.AuthorUserId, createAuthorNames),
                Body: m.Body,
                IsInternal: m.IsInternal,
                IsInitial: m.IsInitial,
                EventType: null,
                Metadata: null))
            .ToList();

        return new CaseDetailDto(
            Id: newCase.Id,
            OrganizationId: organizationId,
            Title: newCase.Title,
            Status: newCase.Status.ToString(),
            Priority: CaseStatusPriorityMapper.ToApiPriorityCode(newCase.Priority),
            SlaState: CaseServiceMappings.ResolveSlaState(newCase),
            IsArchived: newCase.IsArchived,
            SlaDueAtUtc: newCase.SlaDueAtUtc,
            SlaBreachedAtUtc: newCase.SlaBreachedAtUtc,
            SlaPausedAtUtc: newCase.SlaPausedAtUtc,
            SlaRemainingSeconds: newCase.SlaRemainingSeconds,
            RequesterUserId: newCase.RequesterUserId,
            RequesterName: newCase.RequesterName,
            AssigneeUserId: newCase.AssigneeUserId,
            AssigneeName: null,
            CreatedByUserId: newCase.CreatedByUserId,
            CreatedByName: CaseServiceMappings.NormalizeDisplayName(createdByUser.FullName),
            CreatedAtUtc: newCase.CreatedAtUtc,
            UpdatedAtUtc: newCase.UpdatedAtUtc,
            Timeline: timeline);
    }
}
