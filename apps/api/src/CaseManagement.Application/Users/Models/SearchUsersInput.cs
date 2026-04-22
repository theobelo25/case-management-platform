namespace CaseManagement.Application.Users;

public sealed record SearchUsersInput(
    Guid RequesterUserId,
    string? Query,
    string? Cursor,
    int Limit = 20);
