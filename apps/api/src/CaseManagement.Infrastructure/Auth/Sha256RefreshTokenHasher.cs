using System.Security.Cryptography;
using System.Text;
using CaseManagement.Application.Auth.Ports;

namespace CaseManagement.Infrastructure.Auth;

public sealed class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string rawRefreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawRefreshToken);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken));
        
        return Convert.ToHexString(bytes);
    }
}
