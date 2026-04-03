using CaseManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseManagement.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshSessionEntity> RefreshSessions => Set<RefreshSessionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<RefreshSessionEntity>(entity =>
        {
            entity.ToTable("refresh_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.FamilyId).IsRequired();
            entity.HasIndex(x => x.FamilyId);

            entity.Property(x => x.LookupId).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.LookupId).IsUnique();
            entity.Property(x => x.TokenHash).IsRequired();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.ClientUserAgent).HasMaxLength(1024);
            entity.Property(x => x.ClientIpAddress).HasMaxLength(45);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ReplacedBySession)
                .WithMany()
                .HasForeignKey(x => x.ReplacedBySessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
