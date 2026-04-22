using CaseManagement.Api.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace CaseManagement.Api.Realtime;

public interface INotificationsClient
{
    Task NotificationReceived(object payload);
    Task Pong(DateTimeOffset serverTime);
}

public sealed class NotificationsHub : Hub<INotificationsClient>
{
    public async Task Ping()
    {
        await Clients.Caller.Pong(DateTimeOffset.UtcNow);
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.GetUserIdOrNull() is { } userId)
            await Groups.AddToGroupAsync(Context.ConnectionId, NotificationHubGroups.User(userId));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.GetUserIdOrNull() is { } userId)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationHubGroups.User(userId));

        await base.OnDisconnectedAsync(exception);
    }
}