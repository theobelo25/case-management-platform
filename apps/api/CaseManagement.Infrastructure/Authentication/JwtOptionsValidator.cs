using System.Text;
using CaseManagement.Application.Auth.Options;
using Microsoft.Extensions.Options;

namespace CaseManagement.Infrastructure.Authentication;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    /// <summary>
    /// HS256 should use at least 256 bits of key material; UTF-8 byte count matches <see cref="Encoding.UTF8.GetBytes(string)"/> used when signing.
    /// </summary>
    private const int MinSecretUtf8ByteLength = 32;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("JWT options are not configured.");

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Secret))
            failures.Add("JWT secret is missing.");
        else if (Encoding.UTF8.GetByteCount(options.Secret) < MinSecretUtf8ByteLength)
            failures.Add(
                $"JWT secret must be at least {MinSecretUtf8ByteLength} bytes when encoded as UTF-8 (256-bit minimum for HS256).");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            failures.Add("JWT issuer is missing.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            failures.Add("JWT audience is missing.");

        if (options.ExpiryMinutes <= 0)
            failures.Add("JWT expiry minutes must be greater than zero.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
