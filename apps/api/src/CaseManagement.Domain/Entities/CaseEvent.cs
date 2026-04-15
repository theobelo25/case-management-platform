namespace CaseManagement.Domain.Entities;

public class CaseEvent
{
    private CaseEvent() { }

    public Guid Id { get; private set; }
    public Guid CaseId { get; private set; }

    public string Type { get; private set; } = string.Empty;
    public Guid? PerformedByUserId { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static CaseEvent Create(
        Guid caseId,
        string type,
        Guid? performedByUserId,
        string? metadataJson,
        DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Event type is required.", nameof(type));

        return new CaseEvent
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            Type = type.Trim(),
            PerformedByUserId = performedByUserId,
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson,
            CreatedAtUtc = createdAtUtc
        }; 
    }
}