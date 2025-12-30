using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Tenant aggregate
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "shared");

        // Primary key - Vogen value object
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        // Company name
        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        // Subdomain
        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(63);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        // SchemaName - Regular property (set in code, not computed by database)
        builder.Property(t => t.SchemaName)
            .IsRequired()
            .HasMaxLength(100);

        // Subscription tier (value object)
        builder.Property(t => t.Tier)
            .HasConversion(
                tier => tier.Value,
                value => SubscriptionTier.FromValue(value))
            .IsRequired();

        // Subscription status (value object)  
        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Value,
                value => SubscriptionStatus.FromValue(value))
            .IsRequired();

        // Region
        builder.Property(t => t.Region)
            .IsRequired()
            .HasMaxLength(50);

        // Limits
        builder.Property(t => t.MaxUsers)
            .IsRequired();

        builder.Property(t => t.MaxStorageGB)
            .IsRequired();

        builder.Property(t => t.MaxRequestsPerMonth)
            .IsRequired();

        // Dates
        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.LastModifiedDate)
            .IsRequired(false);

        builder.Property(t => t.TrialEndsDate)
            .IsRequired(false);

        // Active flag
        builder.Property(t => t.IsActive)
            .IsRequired();

        // Ignore domain events (not persisted)
        builder.Ignore(t => t.DomainEvents);
    }
}