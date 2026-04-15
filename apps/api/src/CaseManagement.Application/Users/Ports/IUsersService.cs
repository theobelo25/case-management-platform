namespace CaseManagement.Application.Users;

public interface IUsersService
{
    Task<string> GetName(
        Guid userId, 
        CancellationToken cancellationToken = default);
}