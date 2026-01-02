using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Create;

public record CreateTaskCommand(
    ProjectId ProjectId,
    string Title,
    string Description,
    string Priority,
    decimal? EstimatedHours = null,
    DateTime? DueDate = null
) : IRequest<Result<TaskId>>;