using CaseManagement.Api.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace CaseManagement.Api.Tests;

public sealed class ForwardedHeadersConfigurationTests
{
    [Fact]
    public void Apply_when_disabled_does_not_mutate_options()
    {
        var options = new ForwardedHeadersOptions();
        var beforeLimit = options.ForwardLimit;
        var beforeHeaders = options.ForwardedHeaders;

        ForwardedHeadersConfiguration.Apply(
            new ForwardedHeadersSettings { Enabled = false, ForwardLimit = 9 },
            options);

        options.ForwardLimit.Should().Be(beforeLimit);
        options.ForwardedHeaders.Should().Be(beforeHeaders);
    }

    [Fact]
    public void Apply_when_enabled_sets_forwarded_headers_and_forward_limit()
    {
        var options = new ForwardedHeadersOptions();

        ForwardedHeadersConfiguration.Apply(
            new ForwardedHeadersSettings
            {
                Enabled = true,
                ForwardLimit = 3,
                IncludeHostHeader = true
            },
            options);

        options.ForwardLimit.Should().Be(3);
        options.ForwardedHeaders.Should().HaveFlag(ForwardedHeaders.XForwardedFor);
        options.ForwardedHeaders.Should().HaveFlag(ForwardedHeaders.XForwardedProto);
        options.ForwardedHeaders.Should().HaveFlag(ForwardedHeaders.XForwardedHost);
    }

    [Fact]
    public void Apply_parses_known_networks_as_cidr()
    {
        var options = new ForwardedHeadersOptions();

        ForwardedHeadersConfiguration.Apply(
            new ForwardedHeadersSettings
            {
                Enabled = true,
                KnownNetworks =
                [
                    new ForwardedNetworkEntry { Prefix = "203.0.113.0", PrefixLength = 24 }
                ]
            },
            options);

        options.KnownIPNetworks.Should().NotBeEmpty();
    }
}
