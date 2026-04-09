namespace CaseManagement.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public string EmailNormalized { get; private set;} = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public Guid? ActiveOrganizationId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private User()
    {
    }

    private static string NormalizeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return string.Join(" ", name.Split((char[]?)null!, StringSplitOptions.RemoveEmptyEntries));
    }

    public static User Register(
        Guid id, 
        string emailNormalized, 
        string passwordHash, 
        string firstName, 
        string lastName,
        Guid activeOrganizationId, 
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailNormalized);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        if (activeOrganizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization id cannot be empty.", 
                nameof(activeOrganizationId));

        return new User
        {
            Id = id,
            EmailNormalized = emailNormalized.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = NormalizeName(firstName.Trim()),
            LastName = NormalizeName(lastName.Trim()),
            ActiveOrganizationId = activeOrganizationId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void ReplacePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;

        Touched();
    }

    public void ChangeFirstName(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        FirstName = NormalizeName(firstName.Trim());
        
        Touched();
    }

    public void ChangeLastName(string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        LastName = NormalizeName(lastName.Trim());

        Touched();
    }

    public void ChangeActiveOrganization(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization id cannot be empty.", 
                nameof(organizationId));

        ActiveOrganizationId = organizationId;

        Touched();
    }

    private void Touched()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
