using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.ValueObjects;
using TaskEntity = Apex.API.Core.Aggregates.TaskAggregate.Task;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace Apex.API.Infrastructure.Data.Config;

public class TaskActivityLogConfiguration : IEntityTypeConfiguration<TaskActivityLog>
{
    public void Configure(EntityTypeBuilder<TaskActivityLog> builder)
    {
        builder.ToTable("TaskActivityLogs");

        builder.HasKey(tal => tal.Id);

        builder.Property(tal => tal.Id)
            .HasConversion(
                id => id.Value,
                value => (TaskActivityLogId)value)
            .IsRequired();

        builder.Property(tal => tal.TaskId)
            .HasConversion(
                id => id.Value,
                value => (TaskId)value)
            .HasColumnName("TaskId")  // ← Fixes TaskId1 error
            .IsRequired();

        // ✅ FIX: Properly convert TaskActivityType enum to string
        builder.Property(tal => tal.ActivityType)
            .HasConversion(
                v => v.ToString(),           // Enum to string
                v => (TaskActivityType)Enum.Parse(typeof(TaskActivityType), v))  // String to enum
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(tal => tal.Details)
            .HasMaxLength(2000);

        builder.Property(tal => tal.Timestamp)
            .IsRequired();

        builder.HasOne<TaskEntity>()
            .WithMany()
            .HasForeignKey(tal => tal.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tal => tal.TaskId);
        builder.HasIndex(tal => tal.Timestamp);
    }
}
