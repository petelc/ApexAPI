using Ardalis.Specification;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Specifications;

/// <summary>
/// Specification to retrieve all tasks for a project
/// </summary>
public class TasksByProjectSpec : Specification<Core.Aggregates.TaskAggregate.Task>
{
    public TasksByProjectSpec(ProjectId projectId)
    {
        Query
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedDate);
    }

    public TasksByProjectSpec(Guid projectId)
        : this(ProjectId.From(projectId))
    {
    }

    public TasksByProjectSpec(string projectId)
        : this(Guid.Parse(projectId))
    {
    }
}
