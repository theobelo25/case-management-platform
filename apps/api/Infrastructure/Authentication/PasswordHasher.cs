using Isopoh.Cryptography.Argon2;

namespace CaseManagement.Api.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return Argon2.Hash(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return Argon2.Verify(passwordHash, password);
    }
}