using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for ProjectRequest aggregate
/// </summary>
public class ProjectRequestConfiguration : IEntityTypeConfiguration<ProjectRequest>
{
    public void Configure(EntityTypeBuilder<ProjectRequest> builder)
    {
        builder.ToTable("ProjectRequests", "shared");

        // Primary key - Vogen value object
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => ProjectRequestId.From(value))
            .IsRequired();

        // TenantId (Vogen value object) - Multi-tenant isolation
        builder.Property(r => r.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(r => r.TenantId);

        // Composite index for tenant + status queries
        builder.HasIndex(r => new { r.TenantId, r.Status });

        // Index for assigned user queries
        builder.HasIndex(r => r.AssignedToUserId)
            .HasFilter("[AssignedToUserId] IS NOT NULL");

        // Index for project linkage
        builder.HasIndex(r => r.ProjectId)
            .HasFilter("[ProjectId] IS NOT NULL");

        // Core Information
        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        // Status (SmartEnum)
        builder.Property(r => r.Status)
            .HasConversion(
                status => status.Value,
                value => ProjectRequestStatus.FromValue(value))
            .IsRequired();

        // Priority (SmartEnum)
        builder.Property(r => r.Priority)
            .HasConversion(
                priority => priority.Value,
                value => RequestPriority.FromValue(value))
            .IsRequired();

        // User Tracking
        builder.Property(r => r.CreatedByUserId)
            .IsRequired();

        builder.Property(r => r.AssignedToUserId)
            .IsRequired(false);

        builder.Property(r => r.ReviewedByUserId)
            .IsRequired(false);

        builder.Property(r => r.ApprovedByUserId)
            .IsRequired(false);

        builder.Property(r => r.ConvertedByUserId)
            .IsRequired(false);

        // Dates
        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.SubmittedDate)
            .IsRequired(false);

        builder.Property(r => r.ReviewStartedDate)
            .IsRequired(false);

        builder.Property(r => r.ApprovedDate)
            .IsRequired(false);

        builder.Property(r => r.DeniedDate)
            .IsRequired(false);

        builder.Property(r => r.ConvertedDate)
            .IsRequired(false);

        builder.Property(r => r.DueDate)
            .IsRequired(false);

        builder.Property(r => r.LastModifiedDate)
            .IsRequired(false);

        // Notes
        builder.Property(r => r.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.ApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.DenialReason)
            .HasMaxLength(1000);

        // Project linkage
        builder.Property(r => r.ProjectId)
            .IsRequired(false);

        // Ignore domain events (not persisted)
        builder.Ignore(r => r.DomainEvents);
    }
}
