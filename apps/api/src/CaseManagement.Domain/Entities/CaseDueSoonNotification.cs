namespace CaseManagement.Domain.Entities;

public sealed class CaseDueSoonNotification
{
    private CaseDueSoonNotification() { }

    public Guid Id { get; private set; }
    public Guid CaseId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public DateTimeOffset SlaDueAtUtc { get; private set; }
    public int WindowMinutes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static CaseDueSoonNotification Create(
        Guid caseId,
        Guid recipientUserId,
        DateTimeOffset slaDueAtUtc,
        int windowMinutes,
        DateTimeOffset createdAtUtc)
    {
        if (windowMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowMinutes));

        return new CaseDueSoonNotification
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            RecipientUserId = recipientUserId,
            SlaDueAtUtc = slaDueAtUtc,
            WindowMinutes = windowMinutes,
            CreatedAtUtc = createdAtUtc
        };
    }
}
