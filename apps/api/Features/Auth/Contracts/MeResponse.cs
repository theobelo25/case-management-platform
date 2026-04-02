namespace CaseManagement.Api.Features.Auth.Contracts;

public sealed class MeResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}