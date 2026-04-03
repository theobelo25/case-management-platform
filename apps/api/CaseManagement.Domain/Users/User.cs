namespace CaseManagement.Domain.Users;

public sealed class User
{
    public User(Guid id, string email, string firstName, string lastName, string passwordHash, DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
