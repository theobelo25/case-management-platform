namespace CaseManagement.ArchitectureTests;

using NetArchTest.Rules;
using FluentAssertions;
using Xunit;


public class CleanArchitectureTests
{
    private const string DomainNamespace = "CaseManagement.Domain";
    private const string ApplicationNamespace = "CaseManagement.Application";
    private const string InfrastructureNamespace = "CaseManagement.Infrastructure";
    private const string ApiNamespace = "CaseManagement.Api";

    // DOMAIN TESTS
    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_EFCore()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    // APPLICATION TESTS
    [Fact]
    public void Application_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EFCore()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Application_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Application_Should_Not_Contain_Repository_Implementations()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Application.AssemblyMarker).Assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    // INFRASTRUCTURE TESTS
    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Infrastructure.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Infrastructure_DbContexts_Should_Reside_In_Infrastructure()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Infrastructure.AssemblyMarker).Assembly)
            .That()
            .Inherit(typeof(Microsoft.EntityFrameworkCore.DbContext))
            .Should()
            .ResideInNamespaceStartingWith(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Repository_Implementations_Should_Reside_In_Infrastructure()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Infrastructure.AssemblyMarker).Assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceStartingWith(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void EntityTypeConfigurations_Should_Reside_In_Infrastructure()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Infrastructure.AssemblyMarker).Assembly)
            .That()
            .ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .Should()
            .ResideInNamespaceStartingWith(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    // API TESTS
    [Fact]
    public void Controllers_Should_Reside_In_Api()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Api.AssemblyMarker).Assembly)
            .That()
            .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .ResideInNamespaceStartingWith(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }

    [Fact]
    public void Controllers_Should_Not_Depend_On_EFCore()
    {
        var result = Types.InAssembly(typeof(CaseManagement.Api.AssemblyMarker).Assembly)
            .That()
            .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.GetFailingTypes());
    }
}

internal static class NetArchExtensions
{
    public static string GetFailingTypes(this TestResult result)
    {
        if (result.IsSuccessful || result.FailingTypes is null)
            return string.Empty;

        return "Failing types: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
