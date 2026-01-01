using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.DepartmentAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Department aggregate
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "shared");

        // Primary key - Vogen value object
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DepartmentId.From(value))
            .IsRequired();

        // TenantId (Vogen value object) - Multi-tenant isolation
        builder.Property(d => d.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(d => d.TenantId);

        // Composite index for tenant + active status
        builder.HasIndex(d => new { d.TenantId, d.IsActive });

        // Unique constraint on department name per tenant
        builder.HasIndex(d => new { d.TenantId, d.Name })
            .IsUnique();

        // Core Information
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Management
        builder.Property(d => d.DepartmentManagerUserId)
            .IsRequired(false);

        builder.HasIndex(d => d.DepartmentManagerUserId)
            .HasFilter("[DepartmentManagerUserId] IS NOT NULL");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Dates
        builder.Property(d => d.CreatedDate)
            .IsRequired();

        builder.Property(d => d.LastModifiedDate)
            .IsRequired(false);

        // Ignore domain events (not persisted)
        builder.Ignore(d => d.DomainEvents);
    }
}
