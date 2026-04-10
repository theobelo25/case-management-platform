public sealed record UpdateUserProfileInput(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword,
    Guid? ActiveOrganizationId);