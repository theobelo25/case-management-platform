using CaseManagement.Application.Cases;

namespace CaseManagement.Application.Ports;

public interface ICasesService
{
    Task<CaseDetailDto> Create(
        CreateCaseInput input, 
        CancellationToken cancellationToken = default);
}