using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for Tenant aggregate
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table mapping (in shared schema)
        builder.ToTable("Tenants", "shared");

        // Primary key (using Vogen TenantId)
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,           // To database
                value => TenantId.From(value)) // From database
            .ValueGeneratedNever(); // We generate IDs in domain

        // Company information
        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(63);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Subdomain");

        // Schema name (computed column, not stored)
        builder.Ignore(t => t.SchemaName);

        // Subscription tier (store as int, convert to SmartEnum)
        builder.Property(t => t.Tier)
            .IsRequired()
            .HasConversion(
                tier => tier.Value,
                value => SubscriptionTier.FromValue(value));

        // Subscription status (store as int, convert to SmartEnum)
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion(
                status => status.Value,
                value => SubscriptionStatus.FromValue(value));

        // Dates
        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.TrialEndsDate)
            .IsRequired(false);

        builder.Property(t => t.LastModifiedDate)
            .IsRequired(false);

        // Status
        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => new { t.IsActive, t.Status })
            .HasDatabaseName("IX_Tenants_IsActive_Status");

        // Usage limits
        builder.Property(t => t.MaxUsers)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(t => t.MaxRequestsPerMonth)
            .IsRequired()
            .HasDefaultValue(50);

        builder.Property(t => t.MaxStorageGB)
            .IsRequired()
            .HasDefaultValue(1);

        // Region
        builder.Property(t => t.Region)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("USEast");

        // Ignore domain events (they're not persisted)
        builder.Ignore(t => t.DomainEvents);
    }
}
