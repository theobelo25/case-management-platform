namespace CaseManagement.Domain.Entities;

public enum CaseStatus
{
    New = 1,
    Open = 2,
    Pending = 3,
    Resolved = 4,
    Closed = 5
}

public enum CasePriority
{
    Low = 1,
    Medium = 2,
    High = 3
}

public sealed class Case
{
    private readonly List<CaseMessage> _messages = [];
    private readonly List<CaseEvent> _events = [];

    private Case() { }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    
    public Guid OrganizationId { get; private set; }
    public CaseStatus Status { get; private set; }
    public CasePriority Priority { get; private set; }

    public Guid? RequesterUserId { get; private set; }
    public string? RequesterName { get; private set; }

    public Guid? AssigneeUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CaseMessage> Messages => _messages;
    public IReadOnlyCollection<CaseEvent> Events => _events;

    public static Case Create(
        Guid organizationId,
        string title,
        Guid createdByUserId,
        string initialMessageBody,
        CasePriority priority = CasePriority.Medium,
        Guid? requesterUserId = null,
        string? requesterName = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(initialMessageBody))
            throw new ArgumentException("Initial message is required.", nameof(initialMessageBody));

        var now = DateTimeOffset.UtcNow;

        var @case = new Case
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = title.Trim(),
            Status = CaseStatus.New,
            Priority = priority,
            RequesterUserId = requesterUserId,
            RequesterName = string.IsNullOrWhiteSpace(requesterName) ? null : requesterName.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        @case._messages.Add(CaseMessage.CreateInitial(
            caseId: @case.Id,
            authorUserId: createdByUserId,
            body: initialMessageBody,
            createdAtUtc: now));

        return @case;
    }

    public void AddComment(
        Guid authorUserId, string body, bool isInternal)
    {
        if (Status == CaseStatus.Closed)
            throw new InvalidOperationException("Cannot add comments to a closed case.");

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Comment body is required.", nameof(body));

        _messages.Add(CaseMessage.CreateReply(
            caseId: Id,
            authorUserId: authorUserId,
            body: body,
            isInternal: isInternal,
            createdAtUtc: DateTimeOffset.UtcNow));

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AssignTo(Guid assigneeUserId)
    {
        if (Status == CaseStatus.Closed)
            throw new InvalidOperationException("Cannot assign a closed case.");

        AssigneeUserId = assigneeUserId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (Status == CaseStatus.New)
            Status = CaseStatus.Open;
    }

    public void Unassign()
    {
        if (Status == CaseStatus.Closed)
            throw new InvalidOperationException("Cannot unassign a closed case.");

        AssigneeUserId = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangeStatus(CaseStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangePriority(CasePriority priority)
    {
        Priority = priority;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        
        Title = title.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}