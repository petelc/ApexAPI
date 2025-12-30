using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "shared");

        // Primary key configured by Identity
        builder.HasKey(u => u.Id);

        // TenantId (Vogen value object)
        builder.Property(u => u.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(u => u.TenantId);

        // Personal information
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Email configured by Identity, but add index
        builder.HasIndex(u => u.Email);

        // Multi-tenant email uniqueness
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        // Status and dates
        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.CreatedDate)
            .IsRequired();

        builder.Property(u => u.LastLoginDate)
            .IsRequired(false);

        builder.Property(u => u.LastModifiedDate)
            .IsRequired(false);

        // User preferences
        builder.Property(u => u.TimeZone)
            .HasMaxLength(50);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        // Ignore domain events (not persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}
