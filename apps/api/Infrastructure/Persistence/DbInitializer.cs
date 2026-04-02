using CaseManagement.Api.Domain.Users;
using CaseManagement.Api.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext dbContext, 
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordHasher = new PasswordHasher();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@caseplatform.local",
            FirstName = "Demo",
            LastName = "User",
            CreatedAtUtc = DateTime.UtcNow,
            PasswordHash = passwordHasher.Hash("Password123!")
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}