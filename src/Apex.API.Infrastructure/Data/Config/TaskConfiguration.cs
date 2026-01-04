using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Task aggregate
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<Core.Aggregates.TaskAggregate.Task>
{
    public void Configure(EntityTypeBuilder<Core.Aggregates.TaskAggregate.Task> builder)
    {
        builder.ToTable("Tasks", "shared");

        // Primary key - Vogen value object
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TaskId.From(value))
            .IsRequired();

        // TenantId (Vogen value object) - Multi-tenant isolation
        builder.Property(t => t.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(t => t.TenantId);

        // ProjectId (Vogen value object)
        builder.Property(t => t.ProjectId)
            .HasConversion(
                id => id.Value,
                value => ProjectId.From(value))
            .IsRequired();

        builder.HasIndex(t => t.ProjectId);

        // Composite index for project + status queries
        builder.HasIndex(t => new { t.ProjectId, t.Status });

        // Core Information
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(2000);

        // Status (SmartEnum) - ✅ FIXED: Store as string
        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Name,  // ✅ Store name as string
                name => Core.ValueObjects.TaskStatus.FromName(name, ignoreCase: false))  // ✅ Load by name
            .IsRequired()
            .HasMaxLength(50);

        // Priority (SmartEnum) - ✅ FIXED: Store as string
        builder.Property(t => t.Priority)
            .HasConversion(
                priority => priority.Name,  // ✅ Store name as string
                name => RequestPriority.FromName(name, ignoreCase: false))  // ✅ Load by name
            .IsRequired()
            .HasMaxLength(50);

        // Assignment - User
        builder.Property(t => t.AssignedToUserId)
            .IsRequired(false);

        builder.HasIndex(t => t.AssignedToUserId)
            .HasFilter("[AssignedToUserId] IS NOT NULL");

        // Assignment - Department (Vogen value object - NULLABLE)
        builder.Property(t => t.AssignedToDepartmentId)
            .HasConversion(
                id => id.HasValue ? (Guid?)id.Value.Value : null,
                value => value.HasValue ? DepartmentId.From(value.Value) : (DepartmentId?)null)
            .IsRequired(false);

        builder.HasIndex(t => t.AssignedToDepartmentId)
            .HasFilter("[AssignedToDepartmentId] IS NOT NULL");

        // Composite index for department + status queries
        builder.HasIndex(t => new { t.AssignedToDepartmentId, t.Status })
            .HasFilter("[AssignedToDepartmentId] IS NOT NULL");

        // User Tracking
        builder.Property(t => t.CreatedByUserId)
            .IsRequired();

        // Time Tracking
        builder.Property(t => t.EstimatedHours)
            .HasColumnType("decimal(8,2)")
            .IsRequired(false);

        builder.Property(t => t.ActualHours)
            .HasColumnType("decimal(8,2)")
            .IsRequired()
            .HasDefaultValue(0);

        // Dates
        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .IsRequired(false);

        builder.Property(t => t.StartedDate)
            .IsRequired(false);

        builder.Property(t => t.CompletedDate)
            .IsRequired(false);

        builder.Property(t => t.LastModifiedDate)
            .IsRequired(false);

        // Blocking
        builder.Property(t => t.BlockedReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(t => t.BlockedDate)
            .IsRequired(false);

        // Ignore domain events (not persisted)
        builder.Ignore(t => t.DomainEvents);
    }
}