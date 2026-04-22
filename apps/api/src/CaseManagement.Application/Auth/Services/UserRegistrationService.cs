using CaseManagement.Application.Auth.Ports;
using CaseManagement.Application.Common.Ports;
using CaseManagement.Application.Organizations.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Auth;

public sealed class UserRegistrationService : IUserRegistrationService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;
    private readonly IOrganizationManagementRepository _organizationManagement;
    private readonly IOrganizationMembershipRepository _organizationMemberships;
    private readonly IUnitOfWork _unitOfWork;

    public UserRegistrationService(
        IPasswordHasher passwordHasher,
        IUserRepository users,
        TimeProvider time,
        IOrganizationManagementRepository organizationManagement,
        IOrganizationMembershipRepository organizationMemberships,
        IUnitOfWork unitOfWork)
    {
        _passwordHasher = passwordHasher;
        _users = users;
        _time = time;
        _organizationManagement = organizationManagement;
        _organizationMemberships = organizationMemberships;
        _unitOfWork = unitOfWork;
    }

    public async Task<User> Register(
        RegisterUserInput input, 
        CancellationToken cancellationToken = default)
    {
        var organizationName = input.FirstName + "'s Organization";
        var organization = await _organizationManagement.Create(organizationName, cancellationToken);

        var normalized = input.Email.Trim().ToLowerInvariant();

        var user = User.Register(
            Guid.NewGuid(), 
            normalized, 
            _passwordHasher.Hash(input.Password),
            input.FirstName,
            input.LastName,
            organization.Id,
            _time.GetUtcNow());
        
        _users.Add(user);

        await _organizationMemberships.IssueMembership(
            user.Id,
            organization.Id,
            OrganizationRole.Owner,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return user;
    }
}