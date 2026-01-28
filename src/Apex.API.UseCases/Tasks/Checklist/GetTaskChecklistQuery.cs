using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Query to get task checklist items
/// </summary>
public record GetTaskChecklistQuery(
    TaskId TaskId
) : IRequest<Result<List<TaskChecklistItemDto>>>;

/// <summary>
/// DTO for checklist item
/// </summary>
public record TaskChecklistItemDto(
    Guid Id,
    string Description,
    bool IsCompleted,
    int Order,
    Guid? CompletedByUserId,
    DateTime? CompletedDate,
    DateTime CreatedDate
);
