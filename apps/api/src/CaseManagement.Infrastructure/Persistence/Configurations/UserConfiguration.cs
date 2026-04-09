using CaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseManagement.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EmailNormalized)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(e => e.EmailNormalized)
            .IsUnique();

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();
    }
}
