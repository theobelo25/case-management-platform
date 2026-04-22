using CaseManagement.Application.Common.Ports;
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
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseMessage> CaseMessages => Set<CaseMessage>();
    public DbSet<CaseEvent> CaseEvents => Set<CaseEvent>();
    public DbSet<CaseDueSoonNotification> CaseDueSoonNotifications => Set<CaseDueSoonNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CaseManagementDbContext).Assembly);
    }
}