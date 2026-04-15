namespace CaseManagement.Domain.Entities;

public sealed class CaseMessage
{
    private CaseMessage() { }

    public Guid Id { get; private set; }
    public Guid CaseId { get; private set; }
    public Guid AuthorUserId { get; private set; }

    public string Body { get; private set; } = string.Empty;
    
    public bool IsInternal { get; private set; } = false;
    public bool IsInitial { get; private set; } = false;
    
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static CaseMessage CreateInitial(
        Guid caseId,
        Guid authorUserId,
        string body,
        DateTimeOffset createdAtUtc)
    {
        return new CaseMessage
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            AuthorUserId = authorUserId,
            Body = body.Trim(),
            IsInternal = false,
            IsInitial = true,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public static CaseMessage CreateReply(
        Guid caseId,
        Guid authorUserId,
        string body,
        bool isInternal,
        DateTimeOffset createdAtUtc)
    {
        return new CaseMessage
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            AuthorUserId = authorUserId,
            Body = body.Trim(),
            IsInternal = isInternal,
            IsInitial = false,
            CreatedAtUtc = createdAtUtc
        };
    }
}