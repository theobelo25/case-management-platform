using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;

namespace CaseManagement.Application.Auth;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserProfileService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<CurrentUserDto> GetMeAsync(
        Guid userId,
        CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);
        
        return new CurrentUserDto(
            user.Id,
            user.EmailNormalized,
            user.FirstName,
            user.LastName);
    }

    public async Task UpdateProfileAsync(UpdateUserProfileInput input, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(input.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);

        if (input.FirstName is not null)
        {
            if (string.IsNullOrWhiteSpace(input.FirstName))
                throw new BadRequestArgumentException("First name cannot be empty.");
            user.ChangeFirstName(input.FirstName);
        }
        
        if (input.LastName is not null)
        {
            if (string.IsNullOrWhiteSpace(input.LastName))
                throw new BadRequestArgumentException("Last name cannot be empty.");
            user.ChangeLastName(input.LastName);
        }

        var wantsPasswordChange = 
            !string.IsNullOrWhiteSpace(input.NewPassword)
            || !string.IsNullOrWhiteSpace(input.ConfirmNewPassword);
        if (wantsPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(input.CurrentPassword))
                throw new BadRequestArgumentException("Current password is required to set a new password.");

            if (string.IsNullOrWhiteSpace(input.NewPassword))
                throw new BadRequestArgumentException("New password is required.");

            if (!string.Equals(input.NewPassword, input.ConfirmNewPassword, StringComparison.Ordinal))
                throw new BadRequestArgumentException("New password and confirmNewPassword do not match");

            if (!_passwordHasher.Verify(input.CurrentPassword, user.PasswordHash))
                throw new AuthenticationFailedException("Current password is incorrect.");
        
            user.ReplacePasswordHash(_passwordHasher.Hash(input.NewPassword));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
