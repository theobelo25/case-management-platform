namespace CaseManagement.Application.Auth;

public interface IJwtTokenService
{
    JwtTokenResult CreateAccessToken(Guid userId, string email, string fullName);
}
