using CaseManagement.Api.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CaseManagement.Api.Tests;

public sealed class AuthorizationPoliciesTests
{
    [Fact]
    public void ConfigureDefault_sets_fallback_policy_requiring_authenticated_user()
    {
        var services = new ServiceCollection();
        services.AddAuthorization(AuthorizationPolicies.ConfigureDefault);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        options.FallbackPolicy.Should().NotBeNull();
        options.FallbackPolicy!.Requirements.Should().ContainSingle();
        options.FallbackPolicy.Requirements.Single().GetType().Name.Should()
            .Be("DenyAnonymousAuthorizationRequirement");
    }
}
