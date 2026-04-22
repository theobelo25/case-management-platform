namespace CaseManagement.Application.Auth.Ports;

public interface IRefreshTokenHasher
{
    string Hash(string rawRefreshToken);
}
