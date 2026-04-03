using CaseManagement.Api.Validation;
using CaseManagement.Application.Auth;

namespace CaseManagement.Api.Tests.Validation;

public sealed class SignInRequestValidatorTests
{
    private readonly SignInRequestValidator _sut = new();

    [Fact]
    public void Valid_request_has_no_errors()
    {
        var request = new SignInRequest("user@example.com", "correct horse battery staple");

        var result = _sut.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_empty_or_whitespace_has_error(string email)
    {
        var request = new SignInRequest(email, "password");

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Email));
    }

    [Fact]
    public void Email_null_has_error()
    {
        var request = new SignInRequest(null!, "password");

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@nodomain.com")]
    [InlineData("missing-at.com")]
    public void Email_invalid_format_has_error(string email)
    {
        var request = new SignInRequest(email, "password");

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Email));
    }

    [Fact]
    public void Email_exceeds_max_length_has_error()
    {
        var email = $"{new string('a', 316)}@b.co"; // 321 characters

        var request = new SignInRequest(email, "password");

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Password_empty_or_whitespace_has_error(string password)
    {
        var request = new SignInRequest("user@example.com", password);

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Password));
    }

    [Fact]
    public void Password_null_has_error()
    {
        var request = new SignInRequest("user@example.com", null!);

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Password));
    }

    [Fact]
    public void Password_exceeds_max_length_has_error()
    {
        var password = new string('x', 1025);

        var request = new SignInRequest("user@example.com", password);

        var result = _sut.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SignInRequest.Password));
    }

    [Fact]
    public void Password_at_max_length_passes()
    {
        var password = new string('x', 1024);

        var request = new SignInRequest("user@example.com", password);

        var result = _sut.Validate(request);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(SignInRequest.Password));
    }
}
