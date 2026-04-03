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
    
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}