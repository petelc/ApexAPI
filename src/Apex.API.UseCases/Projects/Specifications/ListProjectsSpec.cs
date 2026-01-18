using Apex.API.Core.Aggregates.ProjectAggregate;
using Ardalis.Specification;

namespace Apex.API.UseCases.Projects.Specifications;

/// <summary>
/// Specification for listing projects with filtering and pagination
/// MATCHES your actual ListProjectsQuery parameters
/// </summary>
public class ListProjectsSpec : Specification<Project>
{
    public ListProjectsSpec(
        string? status = null,
        string? priority = null,
        Guid? projectManagerUserId = null,
        Guid? createdByUserId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(p => p.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(p => p.Priority.Name == priority);
        }

        if (projectManagerUserId.HasValue)
        {
            Query.Where(p => p.ProjectManagerUserId == projectManagerUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(p => p.CreatedByUserId == createdByUserId.Value);
        }

        if (startDate.HasValue)
        {
            Query.Where(p => p.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(p => p.EndDate <= endDate.Value);
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

/// <summary>
/// Specification for counting total projects (for pagination)
/// Same filters but no pagination
/// </summary>
public class CountProjectsSpec : Specification<Project>
{
    public CountProjectsSpec(
        string? status = null,
        string? priority = null,
        Guid? projectManagerUserId = null,
        Guid? createdByUserId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        // Apply same filters as ListProjectsSpec (but no pagination)
        if (!string.IsNullOrEmpty(status))
        {
            Query.Where(p => p.Status.Name == status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            Query.Where(p => p.Priority.Name == priority);
        }

        if (projectManagerUserId.HasValue)
        {
            Query.Where(p => p.ProjectManagerUserId == projectManagerUserId.Value);
        }

        if (createdByUserId.HasValue)
        {
            Query.Where(p => p.CreatedByUserId == createdByUserId.Value);
        }

        if (startDate.HasValue)
        {
            Query.Where(p => p.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(p => p.EndDate <= endDate.Value);
        }

        Query.AsNoTracking();
    }
}
