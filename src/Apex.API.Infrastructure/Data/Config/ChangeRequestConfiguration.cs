using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for ChangeRequest aggregate
/// </summary>
public class ChangeRequestConfiguration : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(EntityTypeBuilder<ChangeRequest> builder)
    {
        builder.ToTable("ChangeRequests", "shared");

        // Primary key
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Id)
            .HasConversion(
                id => id.Value,
                value => ChangeRequestId.From(value))
            .ValueGeneratedNever();

        // ⭐ FIX: TenantId conversion
        builder.Property(cr => cr.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        // Required fields
        builder.Property(cr => cr.TenantId)
            .IsRequired();

        builder.Property(cr => cr.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cr => cr.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cr => cr.ImpactAssessment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cr => cr.RollbackPlan)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cr => cr.AffectedSystems)
            .IsRequired()
            .HasMaxLength(1000);

        // Enums stored as strings
        builder.Property(cr => cr.ChangeType)
            .HasConversion(
                ct => ct.Name,
                name => ChangeType.FromName(name, false))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.Status)
            .HasConversion(
                s => s.Name,
                name => ChangeRequestStatus.FromName(name, false))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.Priority)
            .HasConversion(
                p => p.Name,
                name => RequestPriority.FromName(name, false))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.RiskLevel)
            .HasConversion(
                rl => rl.Name,
                name => RiskLevel.FromName(name, false))
            .IsRequired()
            .HasMaxLength(50);

        // Optional fields
        builder.Property(cr => cr.ChangeWindow)
            .HasMaxLength(100);

        builder.Property(cr => cr.ReviewNotes)
            .HasMaxLength(2000);

        builder.Property(cr => cr.ApprovalNotes)
            .HasMaxLength(2000);

        builder.Property(cr => cr.DenialReason)
            .HasMaxLength(1000);

        builder.Property(cr => cr.ImplementationNotes)
            .HasMaxLength(4000);

        builder.Property(cr => cr.RollbackReason)
            .HasMaxLength(1000);

        // User IDs
        builder.Property(cr => cr.CreatedByUserId)
            .IsRequired();

        builder.Property(cr => cr.ReviewedByUserId);
        builder.Property(cr => cr.ApprovedByUserId);
        builder.Property(cr => cr.ProjectId);

        // Dates
        builder.Property(cr => cr.CreatedDate)
            .IsRequired();

        builder.Property(cr => cr.SubmittedDate);
        builder.Property(cr => cr.ReviewStartedDate);
        builder.Property(cr => cr.ApprovedDate);
        builder.Property(cr => cr.DeniedDate);
        builder.Property(cr => cr.ScheduledDate);
        builder.Property(cr => cr.ScheduledStartDate);
        builder.Property(cr => cr.ScheduledEndDate);
        builder.Property(cr => cr.ActualStartDate);
        builder.Property(cr => cr.ActualEndDate);
        builder.Property(cr => cr.CompletedDate);
        builder.Property(cr => cr.FailedDate);
        builder.Property(cr => cr.RolledBackDate);

        // Flags
        builder.Property(cr => cr.RequiresCABApproval)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(cr => cr.TenantId);
        builder.HasIndex(cr => cr.Status);
        builder.HasIndex(cr => cr.ChangeType);
        builder.HasIndex(cr => cr.RiskLevel);
        builder.HasIndex(cr => cr.CreatedByUserId);
        builder.HasIndex(cr => cr.ScheduledStartDate);
        builder.HasIndex(cr => new { cr.TenantId, cr.Status });
        builder.HasIndex(cr => cr.ProjectId);

        // Ignore navigation properties (domain events)
        builder.Ignore(cr => cr.DomainEvents);
    }
}
