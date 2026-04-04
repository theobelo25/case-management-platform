using CaseManagement.Application.Auth.Ports;
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

        var email = "demo@caseplatform.local".Trim().ToLowerInvariant();

        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            return;

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
