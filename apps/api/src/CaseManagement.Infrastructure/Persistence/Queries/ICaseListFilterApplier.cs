using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public interface ICaseListFilterApplier
{
    IQueryable<Case> Apply(IQueryable<Case> query, GetCasesInput input, DateTimeOffset nowUtc);
}
