using CaseManagement.Application.Common;
using CaseManagement.Application.Users;

namespace CaseManagement.Application.Users.Ports;

public interface IUsersSearchQuery
{
    Task<CursorPage<UserSearchResult>> Search(
        string queryString,
        string? cursor,
        int limit = 20,
        CancellationToken cancellationToken = default);
}