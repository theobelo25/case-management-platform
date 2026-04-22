using CaseManagement.Application.Common;
using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Users.Ports;

namespace CaseManagement.Application.Users;

public sealed class UsersService(
    IUsersSearchQuery usersSearch,
    IUserRepository users,
    IUserDisplayNameLookup userDisplayNames) : IUsersService
{
    public async Task<CursorPage<UserSearchResult>> SearchForRequesterAsync(
        SearchUsersInput input,
        CancellationToken cancellationToken = default)
    {
        var requester = await users.GetByIdAsync(input.RequesterUserId, cancellationToken);
        if (requester is null)
            throw new NotFoundException("User not found.", AppErrorCodes.UserNotFound);

        return await usersSearch.Search(
            input.Query ?? string.Empty,
            input.Cursor,
            input.Limit,
            cancellationToken);
    }

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
        return userDisplayNames.GetName(userId, cancellationToken);
    }
}