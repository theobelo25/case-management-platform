namespace CaseManagement.Api.Common.Contracts;

public sealed record PagingQuery
{
    public int Skip { get; init; }
    public int Limit { get; init; } = 10;
}