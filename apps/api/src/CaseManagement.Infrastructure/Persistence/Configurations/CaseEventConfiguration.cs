using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseManagement.Infrastructure.Persistence.Configurations;

public sealed class CaseEventConfiguration : IEntityTypeConfiguration<CaseEvent>
{
    public void Configure(EntityTypeBuilder<CaseEvent> builder)
    {
        builder.ToTable("case_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CaseId)
            .IsRequired();
        
        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.MetadataJson)
            .HasColumnType("jsonb");
        
        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.CaseId, x.CreatedAtUtc });

        builder.HasOne<Case>()
            .WithMany(c => c.Events)
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}