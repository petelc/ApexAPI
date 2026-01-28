using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.TaskAggregate;
using TaskEntity = Apex.API.Core.Aggregates.TaskAggregate.Task;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.Config;

public class TaskChecklistItemConfiguration : IEntityTypeConfiguration<TaskChecklistItem>
{
    public void Configure(EntityTypeBuilder<TaskChecklistItem> builder)
    {
        builder.ToTable("TaskChecklistItems");

        builder.HasKey(ci => ci.Id);

        // ✅ Simple conversion without Create method
        builder.Property(ci => ci.Id)
            .HasConversion(
                id => id.Value,
                value => (TaskChecklistItemId)value)  // Just use the Guid directly
            .IsRequired();

        // ✅ FIX: Explicitly configure TaskId
        builder.Property(ci => ci.TaskId)
            .HasConversion(
                id => id.Value,
                value => (TaskId)value)  // Just use the Guid directly
            .HasColumnName("TaskId")  // ← This fixes TaskId1 error
            .IsRequired();

        builder.Property(ci => ci.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ci => ci.IsCompleted)
            .IsRequired();

        builder.Property(ci => ci.Order)
            .IsRequired();

        // ✅ Configure relationship
        builder.HasOne<TaskEntity>()
            .WithMany()
            .HasForeignKey(ci => ci.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ci => ci.TaskId);
    }
}
