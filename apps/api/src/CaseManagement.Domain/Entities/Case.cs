using System.Text.Json;

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
    public Guid OrganizationId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    
    public CaseStatus Status { get; private set; }
    public CasePriority Priority { get; private set; }

    public Guid? RequesterUserId { get; private set; }
    public string? RequesterName { get; private set; }

    public Guid? AssigneeUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? SlaDueAtUtc { get; private set; }
    public DateTimeOffset? SlaBreachedAtUtc { get; private set; }
    public DateTimeOffset? SlaPausedAtUtc { get; private set; }
    public int? SlaRemainingSeconds { get; private set; }

    public bool IsArchived { get; private set; }

    public IReadOnlyCollection<CaseMessage> Messages => _messages;
    public IReadOnlyCollection<CaseEvent> Events => _events;

    public static Case Create(
        Guid organizationId,
        string title,
        Guid createdByUserId,
        string initialMessageBody,
        SlaDurationPolicy slaPolicy,
        CasePriority priority = CasePriority.Medium,
        Guid? requesterUserId = null,
        string? requesterName = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID is required.", nameof(organizationId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(initialMessageBody))
            throw new ArgumentException("Initial message is required.", nameof(initialMessageBody));

        var now = DateTimeOffset.UtcNow;
        var duration = slaPolicy.ForPriority(priority);

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
            UpdatedAtUtc = now,
            IsArchived = false
        };

        @case._messages.Add(CaseMessage.CreateInitial(
            caseId: @case.Id,
            authorUserId: createdByUserId,
            body: initialMessageBody,
            createdAtUtc: now));

        @case.SlaDueAtUtc = now.Add(duration);
        @case.SlaBreachedAtUtc = null;
        @case.SlaPausedAtUtc = null;
        @case.SlaRemainingSeconds = (int)duration.TotalSeconds;

        @case.AddEvent(
            "sla_due_changed",
            createdByUserId,
            JsonSerializer.Serialize(new
            {
                from = (DateTimeOffset?)null,
                to = @case.SlaDueAtUtc,
                reason = "case_created",
                priority = priority.ToString()
            }));

        return @case;
    }

    public void AddComment(
        Guid authorUserId, string body, bool isInternal)
    {
        EnsureMutable();
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

        var wasNew = Status == CaseStatus.New;
        PromoteNewToOpenIfNeeded(authorUserId);
        if (!wasNew)
            UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AssignTo(
        Guid assigneeUserId,
        Guid? performedByUserId = null,
        string? fromDisplayName = null,
        string? toDisplayName = null)
    {
        EnsureMutable();
        if (Status == CaseStatus.Closed)
            throw new InvalidOperationException("Cannot assign a closed case.");

        var previous = AssigneeUserId;
        AssigneeUserId = assigneeUserId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddEvent(
            "assignee_changed",
            performedByUserId,
            JsonSerializer.Serialize(new
            {
                fromUserId = previous?.ToString(),
                toUserId = assigneeUserId.ToString(),
                fromName = fromDisplayName,
                toName = toDisplayName
            }));

        PromoteNewToOpenIfNeeded(performedByUserId);
    }

    public void Unassign(Guid? performedByUserId = null, string? fromDisplayName = null)
    {
        EnsureMutable();
        if (Status == CaseStatus.Closed)
            throw new InvalidOperationException("Cannot unassign a closed case.");

        var previous = AssigneeUserId;
        AssigneeUserId = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddEvent(
            "assignee_changed",
            performedByUserId,
            JsonSerializer.Serialize(new
            {
                fromUserId = previous?.ToString(),
                toUserId = (string?)null,
                fromName = fromDisplayName,
                toName = (string?)null
            }));
    }

    public void ChangeStatus(CaseStatus status, Guid? performedByUserId = null)
    {
        EnsureMutable();
        if (Status == status)
            return;

        var now = DateTimeOffset.UtcNow;
        var previous = Status;
        Status = status;
        UpdatedAtUtc = now;
        AddEvent(
            "status_changed",
            performedByUserId,
            JsonSerializer.Serialize(new
            {
                from = previous.ToString(),
                to = status.ToString()
            }));
        
        RecalculateSla(
            nowUtc: now,
            reason: "status_changed", 
            performedByUserId: performedByUserId,
            previousStatus: previous,
            slaPolicy: null);
    }

    public void ChangePriority(
        CasePriority priority,
        SlaDurationPolicy slaPolicy,
        Guid? performedByUserId = null)
    {
        EnsureMutable();
        if (Priority == priority)
            return;

        var now = DateTimeOffset.UtcNow;
        var previous = Priority;
        Priority = priority;
        UpdatedAtUtc = now;

        AddEvent(
            "priority_changed",
            performedByUserId,
            JsonSerializer.Serialize(new
            {
                from = previous.ToString(),
                to = priority.ToString()
            }));

        RecalculateSla(
            nowUtc: now,
            reason: "priority_changed",
            performedByUserId: performedByUserId,
            previousPriority: previous,
            slaPolicy: slaPolicy);
    }

    public void Rename(string title)
    {
        EnsureMutable();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        
        Title = title.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Archive(Guid? performedByUserId = null)
    {
        if (IsArchived)
            return;

        IsArchived = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddEvent(
            "case_archived",
            performedByUserId,
            JsonSerializer.Serialize(new { }));
    }

    public void Unarchive(Guid? performedByUserId = null)
    {
        if (!IsArchived)
            return;

        IsArchived = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddEvent(
            "case_unarchived",
            performedByUserId,
            JsonSerializer.Serialize(new { }));
    }

    public bool MarkSlaBreachedIfPastDue(
        DateTimeOffset nowUtc,
        string reason,
        Guid? performedByUserId = null)
    {
        if (SlaDueAtUtc is null)
            return false;

        if (SlaPausedAtUtc is not null || SlaBreachedAtUtc is not null)
            return false;

        if (IsArchived || Status is CaseStatus.Resolved or CaseStatus.Closed)
            return false;

        if (SlaDueAtUtc.Value > nowUtc)
            return false;

        SlaBreachedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        AddEvent("sla_breached", performedByUserId, JsonSerializer.Serialize(new
        {
            dueAt = SlaDueAtUtc,
            breachedAt = SlaBreachedAtUtc,
            reason
        }));

        return true;
    }

    private void PromoteNewToOpenIfNeeded(Guid? performedByUserId)
    {
        if (Status == CaseStatus.New)
            ChangeStatus(CaseStatus.Open, performedByUserId);
    }

    private void EnsureMutable()
    {
        if (IsArchived)
            throw new InvalidOperationException("This case is archived.");
    }

    private void AddEvent(string type, Guid? performedByUserId, string? metadataJson)
    {
        _events.Add(CaseEvent.Create(
            Id,
            type,
            performedByUserId,
            metadataJson,
            DateTimeOffset.UtcNow));
    }

    private void RecalculateSla(
        DateTimeOffset nowUtc,
        string reason,
        Guid? performedByUserId,
        CaseStatus? previousStatus = null,
        CasePriority? previousPriority = null,
        SlaDurationPolicy? slaPolicy = null)
    {
        var previousDue = SlaDueAtUtc;

        var transition = CaseSlaPolicyEngine.Recalculate(
            Status,
            Priority,
            nowUtc,
            new CaseSlaState(SlaDueAtUtc, SlaBreachedAtUtc, SlaPausedAtUtc, SlaRemainingSeconds),
            previousPriority,
            slaPolicy);

        SlaDueAtUtc = transition.State.DueAtUtc;
        SlaBreachedAtUtc = transition.State.BreachedAtUtc;
        SlaPausedAtUtc = transition.State.PausedAtUtc;
        SlaRemainingSeconds = transition.State.RemainingSeconds;

        if (transition.DueChanged)
        {
            AddEvent("sla_due_changed", performedByUserId, JsonSerializer.Serialize(new
            {
                from = previousDue,
                to = SlaDueAtUtc,
                reason
            }));
        }

        if (transition.BreachedNow)
        {
            AddEvent("sla_breached", performedByUserId, JsonSerializer.Serialize(new
            {
                dueAt = SlaDueAtUtc,
                breachedAt = SlaBreachedAtUtc,
                reason
            }));
        }
    }
}