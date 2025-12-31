using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Request aggregate
/// </summary>
public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests", "shared");

        // Primary key - Vogen value object
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => RequestId.From(value))
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
                value => RequestStatus.FromValue(value))
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

        builder.Property(r => r.CompletedByUserId)
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

        builder.Property(r => r.CompletedDate)
            .IsRequired(false);

        builder.Property(r => r.DueDate)
            .IsRequired(false);

        builder.Property(r => r.LastModifiedDate)
            .IsRequired(false);

        // Notes - Updated property names to match Request.cs
        builder.Property(r => r.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.ApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(r => r.DenialReason) // ✅ FIXED: Changed from RejectionReason
            .HasMaxLength(1000);

        builder.Property(r => r.CompletionNotes)
            .HasMaxLength(1000);

        // Ignore domain events (not persisted)
        builder.Ignore(r => r.DomainEvents);
    }
}