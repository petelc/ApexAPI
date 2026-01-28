using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Command to add a checklist item to a task
/// </summary>
public record AddChecklistItemCommand(
    TaskId TaskId,
    string Description,
    int Order
) : IRequest<Result<TaskChecklistItemId>>;
