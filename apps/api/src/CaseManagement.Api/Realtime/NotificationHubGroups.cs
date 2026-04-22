namespace CaseManagement.Api.Realtime;

public static class NotificationHubGroups
{
    public static string User(Guid userId) => $"user:{userId:D}";
}
