using CaseManagement.Application.Ports;
using Isopoh.Cryptography.Argon2;

namespace CaseManagement.Infrastructure.Auth;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int TimeCost = 3;
    private const int MemoryCost = 65536;
    private const int Parallelism = 1;
    private const int HashLength = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Argon2.Hash(
            password,
            TimeCost,
            MemoryCost,
            Parallelism,
            Argon2Type.HybridAddressing,
            HashLength);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return Argon2.Verify(passwordHash, password);
        }
        catch
        {
            return false;
        }
    }
}