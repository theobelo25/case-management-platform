using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseManagement.Infrastructure.Persistence.Configurations;

public sealed class CaseDueSoonNotificationConfiguration : IEntityTypeConfiguration<CaseDueSoonNotification>
{
    public void Configure(EntityTypeBuilder<CaseDueSoonNotification> builder)
    {
        builder.ToTable("case_due_soon_notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CaseId)
            .IsRequired();

        builder.Property(x => x.RecipientUserId)
            .IsRequired();

        builder.Property(x => x.SlaDueAtUtc)
            .IsRequired();

        builder.Property(x => x.WindowMinutes)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.CaseId, x.SlaDueAtUtc, x.WindowMinutes, x.RecipientUserId })
            .IsUnique();

        builder.HasOne<Case>()
            .WithMany()
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
