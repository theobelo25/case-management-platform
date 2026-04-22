using CaseManagement.Api.Cases.Contracts;
using CaseManagement.Application.Cases;
using CaseManagement.Application.Cases.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CaseManagement.Api.Controllers;

internal static class CasesApiValidator
{
    public static bool TryValidateCaseListQuery(
        ModelStateDictionary modelState,
        int limit,
        bool assignedToMe,
        bool unassignedOnly,
        int? dueSoonWithinHours,
        string? sort,
        out string? normalizedSort)
    {
        normalizedSort = null;

        if (limit is < 1 or > 100)
            modelState.AddModelError("limit", "Limit must be between 1 and 100.");

        if (assignedToMe && unassignedOnly)
        {
            modelState.AddModelError(
                "unassignedOnly",
                "assignedToMe and unassignedOnly cannot both be true.");
        }

        if (dueSoonWithinHours is < 1 or > 720)
        {
            modelState.AddModelError(
                "dueSoonWithinHours",
                "dueSoonWithinHours must be between 1 and 720, or omitted.");
        }

        if (!CasesRequestParser.TryParseCaseListSort(sort, out normalizedSort, out var sortError))
            modelState.AddModelError("sort", sortError);

        return modelState.ErrorCount == 0;
    }

    public static bool TryValidateDays(
        ModelStateDictionary modelState,
        string fieldName,
        int days,
        int min = 1,
        int max = 90)
    {
        if (days >= min && days <= max)
            return true;

        modelState.AddModelError(fieldName, $"Days must be between {min} and {max}.");
        return false;
    }

    public static bool TryValidateBulkRequest(
        ModelStateDictionary modelState,
        BulkCasesRequest request,
        out BulkCaseAction action,
        out CasePriorityCode? priorityCode,
        out CaseStatusCode? statusCode)
    {
        action = default;
        priorityCode = null;
        statusCode = null;

        if (request.CaseIds is not { Count: > 0 } || request.CaseIds.Count > 100)
        {
            modelState.AddModelError(nameof(request.CaseIds), "Provide between 1 and 100 case IDs.");
            return false;
        }

        if (!CasesRequestParser.TryParseBulkCaseAction(request.Action, out action))
        {
            modelState.AddModelError(
                nameof(request.Action),
                $"Action must be one of: {CasesRequestParser.SupportedBulkCaseActionWireNamesForMessage}.");
            return false;
        }

        if (action == BulkCaseAction.SetPriority
            && !CaseStatusPriorityMapper.TryParsePriorityCode(request.Priority, out var parsedPriority))
        {
            modelState.AddModelError(
                nameof(request.Priority),
                $"Priority is required and must be {string.Join(", ", Enum.GetNames<CasePriorityCode>())}.");
            return false;
        }
        else if (action == BulkCaseAction.SetPriority)
        {
            CaseStatusPriorityMapper.TryParsePriorityCode(request.Priority, out parsedPriority);
            priorityCode = parsedPriority;
        }

        if (action == BulkCaseAction.SetStatus
            && !CaseStatusPriorityMapper.TryParseStatusCode(request.Status, out var parsedStatus))
        {
            modelState.AddModelError(nameof(request.Status), "Status is required and must be a valid case status code.");
            return false;
        }
        else if (action == BulkCaseAction.SetStatus)
        {
            CaseStatusPriorityMapper.TryParseStatusCode(request.Status, out parsedStatus);
            statusCode = parsedStatus;
        }

        return true;
    }

    public static bool TryValidateUpdateRequest(
        ModelStateDictionary modelState,
        UpdateCaseRequest request,
        out CaseStatusCode statusCode,
        out CasePriorityCode priorityCode)
    {
        statusCode = default;
        priorityCode = default;

        if (!CaseStatusPriorityMapper.TryParseStatusCode(request.Status, out statusCode))
            modelState.AddModelError(
                "status",
                $"Status must be one of: {string.Join(", ", Enum.GetNames<CaseStatusCode>())}.");

        if (!CaseStatusPriorityMapper.TryParsePriorityCode(request.Priority, out priorityCode))
            modelState.AddModelError(
                "priority",
                $"Priority must be one of: {string.Join(", ", Enum.GetNames<CasePriorityCode>())}.");

        return modelState.ErrorCount == 0;
    }
}
