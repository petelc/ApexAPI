using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Ardalis.Specification;

namespace Apex.API.UseCases.ProjectRequests.Specifications;

/// <summary>
/// Specification for listing project requests with filtering and pagination
/// Uses Ardalis.Specification pattern with EfRepository from Traxs.SharedKernel
/// </summary>
public class ListProjectRequestsSpec : Specification<ProjectRequest>
{
    public ListProjectRequestsSpec(
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        Guid? createdByUserId = null,
        bool? isOverdue = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(pr => pr.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(pr => pr.Priority.Name == priority);
        }

        if (assignedToUserId.HasValue)
        {
            Query.Where(pr => pr.AssignedToUserId == assignedToUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(pr => pr.CreatedByUserId == createdByUserId.Value);
        }

        if (isOverdue.HasValue && isOverdue.Value)
        {
            var now = DateTime.UtcNow;
            Query.Where(pr => 
                pr.DueDate.HasValue && 
                pr.DueDate.Value < now &&
                !pr.Status.IsTerminal);
        }

        // Apply ordering
        Query.OrderByDescending(pr => pr.CreatedDate);

        // Apply pagination
        Query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        // Use AsNoTracking for read-only queries
        Query.AsNoTracking();
    }
}

/// <summary>
/// Specification for counting total project requests (for pagination)
/// Same filters but no pagination
/// </summary>
public class CountProjectRequestsSpec : Specification<ProjectRequest>
{
    public CountProjectRequestsSpec(
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        Guid? createdByUserId = null,
        bool? isOverdue = null)
    {
        // Apply same filters as ListProjectRequestsSpec (but no pagination)
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(pr => pr.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(pr => pr.Priority.Name == priority);
        }

        if (assignedToUserId.HasValue)
        {
            Query.Where(pr => pr.AssignedToUserId == assignedToUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(pr => pr.CreatedByUserId == createdByUserId.Value);
        }

        if (isOverdue.HasValue && isOverdue.Value)
        {
            var now = DateTime.UtcNow;
            Query.Where(pr => 
                pr.DueDate.HasValue && 
                pr.DueDate.Value < now &&
                !pr.Status.IsTerminal);
        }

        Query.AsNoTracking();
    }
}
