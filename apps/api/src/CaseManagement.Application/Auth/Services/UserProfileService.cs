using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;

namespace CaseManagement.Application.Auth;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserOrganizationMembershipsQuery _membershipsQuery;

    public UserProfileService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUserOrganizationMembershipsQuery membershipsQuery)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _membershipsQuery = membershipsQuery;
    }

    public async Task<CurrentUserDto> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.", code: AppErrorCodes.UserNotFound);

        var activeOrganizationId = user.ActiveOrganizationId
            ?? throw new NotFoundException("No active organization", AppErrorCodes.NoActiveOrganization);

        var organizations = await _membershipsQuery.ListForUserAsync(
            userId,
            cancellationToken);

        if (organizations.Count == 0)
            throw new NotFoundException("No memberships found", AppErrorCodes.MembershipNotFound); 
            
        return new CurrentUserDto(
            user.Id,
            user.EmailNormalized,
            user.FirstName,
            user.LastName,
            activeOrganizationId,
            organizations);
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

        var orgId = input.ActiveOrganizationId;
        if (orgId is not null)
        {
            if (orgId.Value == Guid.Empty)
                throw new BadRequestArgumentException("Organization id cannot be empty.");

            var isMember = await _membershipsQuery.IsUserMemberOfAsync(
                user.Id,
                orgId.Value,
                cancellationToken);
            if (!isMember)
                throw new BadRequestArgumentException("You are not a member of that organization.");

            user.ChangeActiveOrganization(orgId.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
