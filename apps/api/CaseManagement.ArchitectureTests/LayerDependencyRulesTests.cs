using CaseManagement.Application.Auth;
using CaseManagement.Domain.Users;
using CaseManagement.Infrastructure.Persistence;
using NetArchTest.Rules;

namespace CaseManagement.ArchitectureTests;

/// <summary>
/// Asserts compile-time dependency direction between assemblies (type references), complementing .csproj checks.
/// </summary>
public sealed class LayerDependencyRulesTests
{
    [Fact]
    public void Domain_does_not_reference_outer_layers_or_persistence_frameworks()
    {
        var result = Types
            .InAssembly(typeof(User).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CaseManagement.Application",
                "CaseManagement.Infrastructure",
                "CaseManagement.Api",
                "Microsoft.EntityFrameworkCore",
                "Npgsql.EntityFrameworkCore.PostgreSQL",
                "Microsoft.AspNetCore",
                "Swashbuckle"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FormatDiagnostics(result));
    }

    [Fact]
    public void Application_does_not_reference_Infrastructure_or_Api_or_EF()
    {
        var result = Types
            .InAssembly(typeof(AuthService).Assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CaseManagement.Infrastructure",
                "CaseManagement.Api",
                "Microsoft.EntityFrameworkCore",
                "Npgsql.EntityFrameworkCore.PostgreSQL",
                "Microsoft.AspNetCore"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FormatDiagnostics(result));
    }

    [Fact]
    public void Infrastructure_does_not_reference_Api()
    {
        var result = Types
            .InAssembly(typeof(AppDbContext).Assembly)
            .Should()
            .NotHaveDependencyOn("CaseManagement.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatDiagnostics(result));
    }

    private static string FormatDiagnostics(TestResult result)
    {
        var names = result.FailingTypeNames ?? [];
        return "Violations: " + string.Join(", ", names);
    }
}
