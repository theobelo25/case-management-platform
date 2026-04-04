namespace CaseManagement.Application.Auth.Ports;

public interface IJwtTokenService
{
    JwtTokenResult CreateAccessToken(Guid userId, string email, string fullName);
}
