namespace CaseManagement.Domain.Entities;

public sealed class Organization
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set;}

    public bool IsArchived { get; private set; } = false;

    /// <summary>Business hours to first response for low-priority cases.</summary>
    public int SlaLowHours { get; private set; }

    /// <summary>Business hours to first response for medium-priority cases.</summary>
    public int SlaMediumHours { get; private set; }

    /// <summary>Business hours to first response for high-priority cases.</summary>
    public int SlaHighHours { get; private set; }

    private Organization() { }

    public static Organization Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Organization
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Name = name,
            SlaLowHours = DefaultSlaLowHours,
            SlaMediumHours = DefaultSlaMediumHours,
            SlaHighHours = DefaultSlaHighHours
        };
        
    }

    public const int DefaultSlaLowHours = 24;
    public const int DefaultSlaMediumHours = 8;
    public const int DefaultSlaHighHours = 4;

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Unarchive()
    {
        IsArchived = false;
    }

    public SlaDurationPolicy GetSlaDurationPolicy() => new(SlaLowHours, SlaMediumHours, SlaHighHours);

    public void UpdateSlaPolicy(int lowHours, int mediumHours, int highHours)
    {
        ValidateSlaHour(lowHours, nameof(lowHours));
        ValidateSlaHour(mediumHours, nameof(mediumHours));
        ValidateSlaHour(highHours, nameof(highHours));

        SlaLowHours = lowHours;
        SlaMediumHours = mediumHours;
        SlaHighHours = highHours;
    }

    private static void ValidateSlaHour(int hours, string paramName)
    {
        if (hours is < 1 or > 8760)
            throw new ArgumentOutOfRangeException(
                paramName,
                hours,
                "SLA hour value must be between 1 and 8760.");
    }
}