using CaseManagement.Application.Auth;
using CaseManagement.Application.Auth.Ports;
using Microsoft.Extensions.Configuration;

namespace CaseManagement.Api.Demo;

/// <summary>
/// Optional startup seed for local/demo environments (see <c>Demo:*</c> configuration).
/// </summary>
internal static class DemoDataSeeder
{
    internal static async Task TrySeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["Demo:UserEmail"]?.Trim().ToLowerInvariant() ?? "demo@example.local";
        var password = configuration["Demo:UserPassword"] ?? "DemoPass1!";

        var users = services.GetRequiredService<IUserRepository>();
        if (await users.GetByEmailNormalizedAsync(email, cancellationToken) is not null)
            return;

        var registration = services.GetRequiredService<IUserRegistrationService>();
        await registration.Register(
            new RegisterUserInput(email, password, "Demo", "User"),
            cancellationToken);
    }
}
