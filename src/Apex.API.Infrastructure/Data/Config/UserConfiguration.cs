using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for User (ASP.NET Identity)
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // TenantId (Vogen value object)
        builder.Property(u => u.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(u => u.TenantId);

        // DepartmentId (Vogen value object - NULLABLE) - FIXED! ✅
        builder.Property(u => u.DepartmentId)
            .HasConversion(
                // To database: Extract Guid? from DepartmentId?
                id => id.HasValue ? (Guid?)id.Value.Value : null,
                // From database: Convert Guid? to DepartmentId?
                value => value.HasValue ? DepartmentId.From(value.Value) : (DepartmentId?)null)
            .IsRequired(false);

        builder.HasIndex(u => u.DepartmentId)
            .HasFilter("[DepartmentId] IS NOT NULL");

        // Composite index for tenant + department queries
        builder.HasIndex(u => new { u.TenantId, u.DepartmentId })
            .HasFilter("[DepartmentId] IS NOT NULL");

        // User Details
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Computed column (not stored)
        builder.Ignore(u => u.FullName);

        // User Status
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(u => u.IsActive);

        // Profile
        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(u => u.TimeZone)
            .HasMaxLength(50);

        // Dates
        builder.Property(u => u.CreatedDate)
            .IsRequired();

        builder.Property(u => u.LastModifiedDate)
            .IsRequired(false);

        builder.Property(u => u.LastLoginDate)
            .IsRequired(false);
    }
}