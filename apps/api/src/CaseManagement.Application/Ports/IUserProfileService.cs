namespace CaseManagement.Application.Auth;

public interface IUserProfileService
{
    Task<CurrentUserDto> GetMeAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
        
    Task UpdateProfileAsync(
        UpdateUserProfileInput input, 
        CancellationToken cancellationToken = default);
}