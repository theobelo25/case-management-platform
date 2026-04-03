namespace CaseManagement.Application.Database;

public interface IDevelopmentDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
