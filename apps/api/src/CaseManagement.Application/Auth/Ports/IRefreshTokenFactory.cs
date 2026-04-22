namespace CaseManagement.Application.Auth.Ports;
public sealed record RawRefreshToken(
    string Token, 
    string TokenPrefix, 
    string TokenHash);
    
public interface IRefreshTokenFactory
{
    RawRefreshToken Create();
}