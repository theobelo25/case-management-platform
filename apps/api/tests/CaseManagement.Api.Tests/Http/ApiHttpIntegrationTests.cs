using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CaseManagement.Api.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace CaseManagement.Api.Tests.Http;

[Collection("HttpApi")]
public sealed class ApiHttpIntegrationTests(ApiHttpFixture fixture)
{
    [SkippableFact]
    public async Task Health_returns_OK()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.GetAsync(new Uri("/api/health", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Health_live_returns_OK()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.GetAsync(new Uri("/api/health/live", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Health_ready_returns_OK_when_database_configured()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.GetAsync(new Uri("/api/health/ready", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Auth_me_without_token_returns_401()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.GetAsync(new Uri("/api/auth/me", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task Users_search_without_token_returns_401()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.GetAsync(new Uri("/api/users/search", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task Login_with_invalid_body_returns_400_problem_json()
    {
        Skip.If(fixture.Factory is null, fixture.UnavailableReason ?? "HTTP integration host not available.");

        var response = await fixture.Client!.PostAsJsonAsync(
            new Uri("/api/auth/login", UriKind.Relative),
            new { email = "not-valid", password = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        doc.RootElement.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        doc.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();
    }
}
