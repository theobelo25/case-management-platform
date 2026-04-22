using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public interface ICaseListCursorCodec
{
    string EncodeUpdatedAt(DateTimeOffset updatedAtUtc, Guid id);
    (long UpdatedAtTicks, Guid Id)? TryDecodeUpdatedAt(string? cursor);

    string EncodePriority(CaseListItemReadModel last);
    (CasePriority Priority, Guid Id)? TryDecodePriority(string? cursor);

    string EncodeSla(CaseListItemReadModel last);
    (bool HasDue, DateTimeOffset? DueAt, Guid Id)? TryDecodeSla(string? cursor);
}
