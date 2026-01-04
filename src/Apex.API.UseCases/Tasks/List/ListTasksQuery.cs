using MediatR;
using Ardalis.Result;

namespace Apex.API.UseCases.Tasks.List;

/// <summary>
/// Query to list all tasks for a project
/// </summary>
public record ListTasksQuery(
    Guid ProjectId,
    int PageNumber = 1,
    int PageSize = 100
) : IRequest<Result<PagedResult<TaskDto>>>;

/// <summary>
/// Task DTO for list response
/// </summary>
public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid? AssignedToDepartmentId,
    string? AssignedToDepartmentName,
    decimal? EstimatedHours,  // ✅ Made nullable to match Task entity
    decimal ActualHours,
    DateTime CreatedDate,
    DateTime? StartedDate,
    DateTime? CompletedDate,
    string? BlockedReason
);

/// <summary>
/// Paged result wrapper
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
