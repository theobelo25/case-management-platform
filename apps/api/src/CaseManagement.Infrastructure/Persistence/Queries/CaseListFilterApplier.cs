using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseListFilterApplier : ICaseListFilterApplier
{
    public IQueryable<Case> Apply(IQueryable<Case> query, GetCasesInput input, DateTimeOffset nowUtc)
    {
        query = query.Where(c => c.OrganizationId == input.OrganizationId && !c.IsArchived);

        var assigneeFilter = input.Filters.AssigneeUserId;
        if (assigneeFilter.HasValue)
        {
            var assigneeId = assigneeFilter.Value;
            query = query.Where(c => c.AssigneeUserId == assigneeId);
        }

        var search = input.Filters.Search;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{term}%") ||
                (c.RequesterName != null && EF.Functions.ILike(c.RequesterName, $"%{term}%")));
        }

        var parsedPriority = CaseStatusPriorityMapper.ParsePriorityFilter(input.Filters.Priority);
        if (parsedPriority is not null)
            query = query.Where(c => c.Priority == parsedPriority.Value);

        var statuses = CaseStatusPriorityMapper.ParseStatusFilter(input.Filters.Status);
        if (statuses is { Length: > 0 })
            query = query.Where(c => statuses.Contains(c.Status));

        if (input.Filters.OverdueOnly)
        {
            query = query.Where(c =>
                c.SlaDueAtUtc != null &&
                c.SlaDueAtUtc <= nowUtc &&
                c.Status != CaseStatus.Resolved &&
                c.Status != CaseStatus.Closed);
        }

        if (input.Filters.BreachedOnly)
            query = query.Where(c => c.SlaBreachedAtUtc != null);

        if (input.Filters.UnassignedOnly)
            query = query.Where(c => c.AssigneeUserId == null);

        var dueSoonWithinHours = input.Filters.DueSoonWithinHours;
        if (dueSoonWithinHours is { } hours && hours > 0)
        {
            var windowEnd = nowUtc.AddHours(hours);
            query = query.Where(c =>
                c.SlaDueAtUtc != null &&
                c.SlaDueAtUtc > nowUtc &&
                c.SlaDueAtUtc <= windowEnd &&
                c.Status != CaseStatus.Resolved &&
                c.Status != CaseStatus.Closed);
        }

        return query;
    }
}
