namespace CaseManagement.Domain.Entities;

public sealed class Organization
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set;}

    public bool IsArchived { get; private set; } = false;

    private Organization() { }

    public static Organization Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Organization
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Name = name
        };
        
    }

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
}