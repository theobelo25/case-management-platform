using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Extensions;

public readonly record struct UserContext(Guid UserId, Guid? ActiveOrganizationId)
{
    public string? ActiveOrganizationIdValue => ActiveOrganizationId?.ToString();
}

public static class ControllerUserContextExtensions
{
    public static bool TryGetUserContext(this ControllerBase controller, out UserContext context)
    {
        var userId = controller.User.GetUserIdOrNull();
        if (userId is not { } id)
        {
            context = default;
            return false;
        }

        context = new UserContext(id, controller.User.GetActiveOrganizationIdOrNull());
        return true;
    }
}
