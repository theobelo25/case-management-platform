using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseManagement.Infrastructure.Persistence.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RequesterName)
            .HasMaxLength(200);
        
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.SlaDueAtUtc);

        builder.Property(x => x.SlaBreachedAtUtc);
        
        builder.Property(x => x.SlaPausedAtUtc);

        builder.Property(x => x.SlaRemainingSeconds);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(nameof(CaseMessage.CaseId))
            .OnDelete(DeleteBehavior.Cascade);
    }
}