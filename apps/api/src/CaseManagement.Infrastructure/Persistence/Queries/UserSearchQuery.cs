using System.Text;
using CaseManagement.Application.Common;
using CaseManagement.Application.Users.Ports;
using CaseManagement.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class UsersSearchQuery(
    CaseManagementDbContext db
) : IUsersSearchQuery
{
    private sealed record UserSearchRow(
        Guid Id,
        string FirstName,
        string LastName,
        string FullName,
        string Email);

    public async Task<CursorPage<UserSearchResult>> Search(
        string queryString,
        string? cursor,
        int limit = 20, 
        CancellationToken cancellationToken = default)
    {
        queryString = queryString?.Trim() ?? string.Empty;
        limit = Math.Clamp(limit, 1, 100);

        if (queryString.Length == 0)
        {
            return new CursorPage<UserSearchResult>(
                Array.Empty<UserSearchResult>(),
                string.Empty,
                limit);
        }

        
        var pattern = $"%{queryString}%";

        // FullName is not a mapped column; use a translatable concatenation for search and projection.
        var baseQuery = db.Users
            .AsNoTracking()
            .Where(u =>
                EF.Functions.ILike(u.FirstName, pattern) ||
                EF.Functions.ILike(u.LastName, pattern) ||
                EF.Functions.ILike(u.FirstName + " " + u.LastName, pattern) ||
                EF.Functions.ILike(u.EmailNormalized, pattern));
        
        var decoded = TryDecodeCursor(cursor);
        if (decoded is not null)
        {
            var (cursorFirst, cursorLast, cursorId) = decoded.Value;

            baseQuery = baseQuery.Where(u =>
                u.FirstName.CompareTo(cursorFirst) > 0 ||
                (u.FirstName == cursorFirst && u.LastName.CompareTo(cursorLast) > 0) ||
                (u.FirstName == cursorFirst && u.LastName == cursorLast && u.Id.CompareTo(cursorId) > 0));
        }

        var rows = await baseQuery
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.Id)
            .Select(u => new UserSearchRow(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.EmailNormalized))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);
        
        var hasMore = rows.Count > limit;
        var pageRows = hasMore ? rows.Take(limit).ToArray() : rows.ToArray();

        var items = pageRows
            .Select(r => new UserSearchResult(r.Id, r.FullName, r.Email))
            .ToArray();

        var nextCursor = hasMore
            ? EncodeCursor(pageRows[^1].FirstName, pageRows[^1].LastName, pageRows[^1].Id)
            : string.Empty;

        return new CursorPage<UserSearchResult>(
            items,
            nextCursor,
            limit);
    }

    private static string EncodeCursor(string firstName, string lastName, Guid id)
    {
        firstName ??= string.Empty;
        lastName ??= string.Empty;
        // Tab-delimited payload: FirstName \t LastName \t Guid(N)
        var payload = $"{firstName}\t{lastName}\t{id:N}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        return Convert.ToBase64String(bytes);
    }

    private static (string FirstName, string LastName, Guid Id)? TryDecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var payload = Encoding.UTF8.GetString(bytes);
            // Expect exactly 3 parts: firstName, lastName, guid
            var parts = payload.Split('\t');
            if (parts.Length != 3)
                return null;
            var firstName = parts[0];
            var lastName = parts[1];
            if (!Guid.TryParseExact(parts[2], "N", out var id))
                return null;
            return (firstName, lastName, id);
        }
        catch (FormatException)
        {
            // Invalid Base64
            return null;
        }
        catch (ArgumentException)
        {
            // Invalid UTF8 or malformed input edge cases
            return null;
        }
    }
}