using CaseManagement.Application.Common;
using CaseManagement.Application.Ports;

namespace CaseManagement.Application.Users;

public sealed class UsersService(
    IUsersSearchQuery usersSearch,
    IUserRepository users) : IUsersService
{
    public Task<CursorPage<UserSearchResult>> Search(
        string queryString,
        string? cursor,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return usersSearch.Search(queryString, cursor, limit, cancellationToken);
    }

    public Task<string> GetName(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return users.GetName(userId, cancellationToken);
    }
}