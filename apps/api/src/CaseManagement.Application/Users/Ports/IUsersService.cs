using CaseManagement.Application.Common;
using CaseManagement.Application.Users;

namespace CaseManagement.Application.Users.Ports;

public interface IUsersService
{
    Task<CursorPage<UserSearchResult>> SearchForRequesterAsync(
        SearchUsersInput input,
        CancellationToken cancellationToken = default);

    Task<CursorPage<UserSearchResult>> Search(
        string queryString,
        string? cursor,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<string> GetName(
        Guid userId, 
        CancellationToken cancellationToken = default);
}