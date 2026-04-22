namespace CaseManagement.Application.Auth.Ports;
public interface IAccessTokenIssuer
{
    string CreateAccessToken(
        Guid userId,
        string emailNormalized,
        string firstName,
        string lastName);
}
