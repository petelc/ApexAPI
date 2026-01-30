using Ardalis.Specification;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Specifications;

/// <summary>
/// Specification to retrieve a task by its ID
/// </summary>
public class TaskByIdSpec : Specification<Core.Aggregates.TaskAggregate.Task>
{
    public TaskByIdSpec(TaskId taskId)
    {
        Query.Where(t => t.Id == taskId);
    }

    public TaskByIdSpec(Guid taskId)
        : this(TaskId.From(taskId))
    {
    }

    public TaskByIdSpec(string taskId)
        : this(Guid.Parse(taskId))
    {
    }
}
