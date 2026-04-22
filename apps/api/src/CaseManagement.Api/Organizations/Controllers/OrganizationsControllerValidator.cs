using CaseManagement.Api.Common.Contracts;
using CaseManagement.Api.Organizations.Contracts;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CaseManagement.Api.Controllers;

internal static class OrganizationsControllerValidator
{
    public static bool TryValidatePagingQuery(ModelStateDictionary modelState, PagingQuery query)
    {
        if (query.Skip < 0)
            modelState.AddModelError(nameof(query.Skip), "Skip must be greater than or equal to 0.");

        if (query.Limit is < 1 or > 100)
            modelState.AddModelError(nameof(query.Limit), "Limit must be between 1 and 100.");

        return modelState.ErrorCount == 0;
    }

    public static bool TryValidateCreateRequest(
        ModelStateDictionary modelState,
        CreateOrganizationRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            modelState.AddModelError(nameof(body.Name), "Organization name is required.");

        return modelState.ErrorCount == 0;
    }

    public static bool TryValidateSlaPolicyRequest(
        ModelStateDictionary modelState,
        UpdateOrganizationSlaPolicyRequest body)
    {
        ValidateSlaHour(modelState, nameof(body.LowHours), body.LowHours);
        ValidateSlaHour(modelState, nameof(body.MediumHours), body.MediumHours);
        ValidateSlaHour(modelState, nameof(body.HighHours), body.HighHours);
        return modelState.ErrorCount == 0;
    }

    public static bool TryValidateTransferOwnershipRequest(
        ModelStateDictionary modelState,
        TransferOwnershipRequest body)
    {
        if (body.NewOwnerUserId == Guid.Empty)
            modelState.AddModelError(nameof(body.NewOwnerUserId), "New owner user ID is required.");

        return modelState.ErrorCount == 0;
    }

    private static void ValidateSlaHour(ModelStateDictionary modelState, string fieldName, int value)
    {
        if (value is < 1 or > 8760)
            modelState.AddModelError(fieldName, "Each SLA value must be between 1 and 8760 hours.");
    }
}
