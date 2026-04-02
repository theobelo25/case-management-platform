using CaseManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordHasher = new PasswordHasher<User>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@caseplatform.local",
            FirstName = "Demo",
            LastName = "User",
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}