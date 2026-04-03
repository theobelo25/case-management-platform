using CaseManagement.Application.Auth;
using CaseManagement.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
            return;

        var email = "demo@caseplatform.local".Trim().ToLowerInvariant();

        await userRepository.AddAsync(
            new User(
                Guid.NewGuid(),
                email,
                "Demo",
                "User",
                passwordHasher.Hash("Password123!"),
                DateTime.UtcNow),
            cancellationToken);
    }
}