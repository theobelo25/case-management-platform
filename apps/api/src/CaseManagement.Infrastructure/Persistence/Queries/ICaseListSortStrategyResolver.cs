namespace CaseManagement.Infrastructure.Persistence.Queries;

public interface ICaseListSortStrategyResolver
{
    ICaseListSortStrategy Resolve(string? sortField);
}
