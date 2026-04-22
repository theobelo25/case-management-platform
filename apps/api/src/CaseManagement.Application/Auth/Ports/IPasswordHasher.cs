namespace CaseManagement.Application.Auth.Ports;
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}