using Microsoft.Extensions.Options;

namespace CaseManagement.Infrastructure.Authentication;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("JWT options are not configured.");

        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Secret))
            failures.Add("JWT secret is missing.");
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
