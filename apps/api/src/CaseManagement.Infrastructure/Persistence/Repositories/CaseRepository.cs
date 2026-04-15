using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using CaseManagement.Infrastructure.Persistence;

namespace CaseManagement.Infrastructure.Cases.Repositories;

public sealed class CaseRepository(
    CaseManagementDbContext db
) : ICaseRepository
{
    public void Add(
        Case @case) =>
        db.Cases.Add(@case);
}