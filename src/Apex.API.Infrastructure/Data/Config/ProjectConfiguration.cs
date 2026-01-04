using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Project aggregate
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "shared");

        // Primary key - Vogen value object
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProjectId.From(value))
            .IsRequired();

        // TenantId (Vogen value object) - Multi-tenant isolation
        builder.Property(p => p.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(p => p.TenantId);

        // Composite index for tenant + status queries
        builder.HasIndex(p => new { p.TenantId, p.Status });

        // Index for project manager queries
        builder.HasIndex(p => p.ProjectManagerUserId)
            .HasFilter("[ProjectManagerUserId] IS NOT NULL");

        // Index for project request linkage
        builder.HasIndex(p => p.ProjectRequestId);

        // Core Information
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        // Status (SmartEnum) - ✅ FIXED: Store as string
        builder.Property(p => p.Status)
            .HasConversion(
                status => status.Name,  // ✅ Store name as string
                name => ProjectStatus.FromName(name, ignoreCase: false))  // ✅ Load by name
            .IsRequired()
            .HasMaxLength(50);

        // Priority (SmartEnum) - ✅ FIXED: Store as string
        builder.Property(p => p.Priority)
            .HasConversion(
                priority => priority.Name,  // ✅ Store name as string
                name => RequestPriority.FromName(name, ignoreCase: false))  // ✅ Load by name
            .IsRequired()
            .HasMaxLength(50);

        // Link to ProjectRequest
        builder.Property(p => p.ProjectRequestId)
            .IsRequired();

        // Budget & Timeline
        builder.Property(p => p.Budget)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(p => p.StartDate)
            .IsRequired(false);

        builder.Property(p => p.EndDate)
            .IsRequired(false);

        builder.Property(p => p.ActualStartDate)
            .IsRequired(false);

        builder.Property(p => p.ActualEndDate)
            .IsRequired(false);

        // User Tracking
        builder.Property(p => p.CreatedByUserId)
            .IsRequired();

        builder.Property(p => p.ProjectManagerUserId)
            .IsRequired(false);

        // Dates
        builder.Property(p => p.CreatedDate)
            .IsRequired();

        builder.Property(p => p.LastModifiedDate)
            .IsRequired(false);

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}