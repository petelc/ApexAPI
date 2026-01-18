using Apex.API.Core.Aggregates.TaskAggregate;
using Ardalis.Specification;

namespace Apex.API.UseCases.Tasks.Specifications;

/// <summary>
/// Specification for listing tasks with filtering and pagination
/// MATCHES your actual ListTasksQuery parameters
/// </summary>
public class ListTasksSpec : Specification<Core.Aggregates.TaskAggregate.Task>
{
    public ListTasksSpec(
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        Guid? createdByUserId = null,
        DateTime? dueDate = null,
        DateTime? createdDate = null,
        DateTime? startedDate = null,
        DateTime? completedDate = null,
        DateTime? lastModifiedDate = null,
        DateTime? blockedDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(t => t.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(t => t.Priority.Name == priority);
        }

        if (assignedToUserId.HasValue)
        {
            Query.Where(t => t.AssignedToUserId == assignedToUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(t => t.CreatedByUserId == createdByUserId.Value);
        }

        if (dueDate.HasValue)
        {
            Query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == dueDate.Value.Date);
        }

        if (createdDate.HasValue)
        {
            Query.Where(t => t.CreatedDate.Date == createdDate.Value.Date);
        }

        if (startedDate.HasValue)
        {
            Query.Where(t => t.StartedDate.HasValue && t.StartedDate.Value.Date == startedDate.Value.Date);
        }

        if (completedDate.HasValue)
        {
            Query.Where(t => t.CompletedDate.HasValue && t.CompletedDate.Value.Date == completedDate.Value.Date);
        }

        if (lastModifiedDate.HasValue)
        {
            Query.Where(t => t.LastModifiedDate.HasValue && t.LastModifiedDate.Value.Date == lastModifiedDate.Value.Date);
        }

        if (blockedDate.HasValue)
        {
            Query.Where(t => t.BlockedDate.HasValue && t.BlockedDate.Value.Date == blockedDate.Value.Date);
        }

        // Apply ordering
        Query.OrderByDescending(p => p.CreatedDate);

        // Apply pagination
        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        // Use AsNoTracking for read-only queries
        Query.AsNoTracking();
    }
}

public class CountTasksSpec : Specification<Core.Aggregates.TaskAggregate.Task>
{
    public CountTasksSpec(
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        Guid? createdByUserId = null,
        DateTime? dueDate = null,
        DateTime? createdDate = null,
        DateTime? startedDate = null,
        DateTime? completedDate = null,
        DateTime? lastModifiedDate = null,
        DateTime? blockedDate = null)
    {
        // Apply same filters as ListTasksSpec (but no pagination)
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(t => t.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(t => t.Priority.Name == priority);
        }

        if (assignedToUserId.HasValue)
        {
            Query.Where(t => t.AssignedToUserId == assignedToUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(t => t.CreatedByUserId == createdByUserId.Value);
        }

        if (dueDate.HasValue)
        {
            Query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == dueDate.Value.Date);
        }

        if (createdDate.HasValue)
        {
            Query.Where(t => t.CreatedDate.Date == createdDate.Value.Date);
        }

        if (startedDate.HasValue)
        {
            Query.Where(t => t.StartedDate.HasValue && t.StartedDate.Value.Date == startedDate.Value.Date);
        }

        if (completedDate.HasValue)
        {
            Query.Where(t => t.CompletedDate.HasValue && t.CompletedDate.Value.Date == completedDate.Value.Date);
        }

        if (lastModifiedDate.HasValue)
        {
            Query.Where(t => t.LastModifiedDate.HasValue && t.LastModifiedDate.Value.Date == lastModifiedDate.Value.Date);
        }

        if (blockedDate.HasValue)
        {
            Query.Where(t => t.BlockedDate.HasValue && t.BlockedDate.Value.Date == blockedDate.Value.Date);
        }

        Query.AsNoTracking();
    }
}