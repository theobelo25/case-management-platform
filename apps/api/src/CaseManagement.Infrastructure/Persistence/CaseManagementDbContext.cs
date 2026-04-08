using CaseManagement.Application.Ports;
using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public sealed class CaseManagementDbContext : DbContext, IUnitOfWork
{
    public CaseManagementDbContext(
        DbContextOptions<CaseManagementDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CaseManagementDbContext).Assembly);
    }
}