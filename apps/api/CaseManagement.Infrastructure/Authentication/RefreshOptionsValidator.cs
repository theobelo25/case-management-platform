using System.Text;
using CaseManagement.Application.Auth.Options;
using Microsoft.Extensions.Options;

namespace CaseManagement.Infrastructure.Authentication;

public sealed class RefreshTokenOptionsValidator : IValidateOptions<RefreshTokenOptions>
{
    public ValidateOptionsResult Validate(string? name, RefreshTokenOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("Refresh token options are not configured.");
        
        var failures = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.CookieName))
            failures.Add("Refresh token cookie name is missing.");
        
        if (options.ExpiryDays <= 0)
            failures.Add("Refresh token expiry days must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(options.CookiePath))
            failures.Add("Refresh token cookie path is missing."); // if you require it
        
        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
