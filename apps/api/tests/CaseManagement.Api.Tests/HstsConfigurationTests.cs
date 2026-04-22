using CaseManagement.Api.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.HttpsPolicy;

namespace CaseManagement.Api.Tests;

public sealed class HstsConfigurationTests
{
    [Fact]
    public void Apply_when_disabled_does_not_mutate_options()
    {
        var options = new HstsOptions();
        var beforeAge = options.MaxAge;

        HstsConfiguration.Apply(
            new HstsSettings { Enabled = false, MaxAgeDays = 99 },
            options);

        options.MaxAge.Should().Be(beforeAge);
    }

    [Fact]
    public void Apply_when_enabled_sets_max_age_and_flags()
    {
        var options = new HstsOptions();

        HstsConfiguration.Apply(
            new HstsSettings
            {
                Enabled = true,
                MaxAgeDays = 180,
                IncludeSubDomains = true,
                Preload = false
            },
            options);

        options.MaxAge.Should().Be(TimeSpan.FromDays(180));
        options.IncludeSubDomains.Should().BeTrue();
        options.Preload.Should().BeFalse();
    }

    [Fact]
    public void Apply_adds_excluded_hosts()
    {
        var options = new HstsOptions();

        HstsConfiguration.Apply(
            new HstsSettings
            {
                Enabled = true,
                MaxAgeDays = 30,
                ExcludedHosts = [" localhost ", "127.0.0.1"]
            },
            options);

        options.ExcludedHosts.Should().Contain("localhost");
        options.ExcludedHosts.Should().Contain("127.0.0.1");
    }
}
