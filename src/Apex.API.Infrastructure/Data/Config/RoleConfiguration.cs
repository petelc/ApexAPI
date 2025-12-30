using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Role entity
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "shared");

        // Primary key configured by Identity
        builder.HasKey(r => r.Id);

        // TenantId (Vogen value object)
        builder.Property(r => r.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(r => r.TenantId);

        // Role information
        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .IsRequired();

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        // Multi-tenant role name uniqueness
        builder.HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();
    }
}
