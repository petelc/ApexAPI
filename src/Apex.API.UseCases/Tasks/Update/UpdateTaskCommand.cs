using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Update;

/// <summary>
/// Command to update task details
/// </summary>
public record UpdateTaskCommand(
    TaskId TaskId,
    string Title,
    string Description,
    string Priority,
    decimal? EstimatedHours = null,
    DateTime? DueDate = null
) : IRequest<Result>;
