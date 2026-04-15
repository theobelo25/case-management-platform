using CaseManagement.Application.Cases;
using CaseManagement.Application.Common;
using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface ICaseRepository
{
    void Add(
        Case @case);

    Task<CursorPage<Case>> GetCases(
        GetCasesInput input,
        CancellationToken cancellationToken = default);
}