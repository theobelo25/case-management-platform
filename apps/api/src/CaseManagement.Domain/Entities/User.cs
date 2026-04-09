namespace CaseManagement.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public string EmailNormalized { get; private set;} = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

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
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailNormalized);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new User
        {
            Id = id,
            EmailNormalized = emailNormalized.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = NormalizeName(firstName.Trim()),
            LastName = NormalizeName(lastName.Trim()),
            CreatedAtUtc = createdAtUtc
        };
    }

    public void ReplacePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public void ChangeFirstName(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        FirstName = NormalizeName(firstName.Trim());
    }

    public void ChangeLastName(string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        LastName = NormalizeName(lastName.Trim());
    }
}
