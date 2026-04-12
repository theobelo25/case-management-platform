using CaseManagement.Application.Exceptions;
using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Organizations;

public sealed class CreateOrganizationService(
    IUserRepository users,
    IOrganizationsRepository organizations,
    IUnitOfWork unitOfWork
) : ICreateOrganizationService
{
    public async Task<OrganizationResult> CreateOrganizationAndSetOwner(
        Guid userId,
        string? name,
        CancellationToken cancellationToken = default)
    {
            var user = await users.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("user not found.", code: AppErrorCodes.UserNotFound);

            var orgName = !string.IsNullOrWhiteSpace(name) ? name : $"{user.FirstName}'s Organization";

            var organization = await organizations.Create(orgName, cancellationToken);
            await organizations.IssueMembership(
                userId,
                organization.Id,
                OrganizationRole.Owner,
                cancellationToken
            );

            user.ChangeActiveOrganization(organization.Id);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new OrganizationResult(
                organization.Id,
                organization.Name,
                organization.CreatedAtUtc,
                organization.IsArchived);
    } 
}