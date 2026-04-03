namespace CaseManagement.Application.Auth;

public sealed record SignOutRequest(bool RevokeAllSessions = false);