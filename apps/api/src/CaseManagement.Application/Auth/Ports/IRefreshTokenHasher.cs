namespace CaseManagement.Application.Ports;

public interface IRefreshTokenHasher
{
    string Hash(string rawRefreshToken);
}
