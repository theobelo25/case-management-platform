using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseManagement.Infrastructure.Persistence.Configurations;

public class CaseMessageConfiguration : IEntityTypeConfiguration<CaseMessage>
{
    public void Configure(EntityTypeBuilder<CaseMessage> builder)
    {
        builder.ToTable("case_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Body)
            .HasMaxLength(10000)
            .IsRequired();
        
        builder.Property(x => x.IsInternal)
            .IsRequired();

        builder.Property(x => x.IsInitial)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.CaseId, x.CreatedAtUtc});

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}