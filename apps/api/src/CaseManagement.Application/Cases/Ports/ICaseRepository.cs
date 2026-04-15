using CaseManagement.Domain.Entities;

namespace CaseManagement.Application.Ports;

public interface ICaseRepository
{
    void Add(
        Case @case);
}