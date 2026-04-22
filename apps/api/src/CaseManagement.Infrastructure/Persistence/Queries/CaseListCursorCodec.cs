using System.Text;
using CaseManagement.Application.Cases;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Infrastructure.Persistence.Queries;

public sealed class CaseListCursorCodec : ICaseListCursorCodec
{
    public string EncodeUpdatedAt(DateTimeOffset updatedAtUtc, Guid id)
    {
        var payload = $"{updatedAtUtc.UtcTicks}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    public (long UpdatedAtTicks, Guid Id)? TryDecodeUpdatedAt(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|');
            if (parts.Length != 2)
                return null;
            if (!long.TryParse(parts[0], out var ticks))
                return null;
            if (!Guid.TryParseExact(parts[1], "N", out var id))
                return null;
            return (ticks, id);
        }
        catch
        {
            return null;
        }
    }

    public string EncodePriority(CaseListItemReadModel last)
    {
        var payload = $"PRI|{(int)last.Priority}|{last.Id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    public (CasePriority Priority, Guid Id)? TryDecodePriority(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', StringSplitOptions.None);
            if (parts.Length != 3 || !string.Equals(parts[0], "PRI", StringComparison.Ordinal))
                return null;

            if (!int.TryParse(parts[1], out var pInt) || !Enum.IsDefined(typeof(CasePriority), pInt))
                return null;

            if (!Guid.TryParseExact(parts[2], "N", out var id))
                return null;

            return ((CasePriority)pInt, id);
        }
        catch
        {
            return null;
        }
    }

    public string EncodeSla(CaseListItemReadModel last)
    {
        if (last.SlaDueAtUtc is { } due)
        {
            var payload = $"SLA|1|{due.UtcTicks}|{last.Id:N}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        }

        var nullPayload = $"SLA|0||{last.Id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(nullPayload));
    }

    public (bool HasDue, DateTimeOffset? DueAt, Guid Id)? TryDecodeSla(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', StringSplitOptions.None);
            if (parts.Length != 4 || !string.Equals(parts[0], "SLA", StringComparison.Ordinal))
                return null;

            var hasDue = string.Equals(parts[1], "1", StringComparison.Ordinal);
            if (!Guid.TryParseExact(parts[3], "N", out var id))
                return null;

            if (hasDue)
            {
                if (!long.TryParse(parts[2], out var ticks))
                    return null;
                var due = new DateTimeOffset(ticks, TimeSpan.Zero);
                return (true, due, id);
            }

            if (!string.IsNullOrEmpty(parts[2]))
                return null;

            return (false, null, id);
        }
        catch
        {
            return null;
        }
    }
}
