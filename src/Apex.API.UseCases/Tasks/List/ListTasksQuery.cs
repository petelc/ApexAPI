using MediatR;
using Ardalis.Result;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.List;

/// <summary>
/// Query to list all tasks for a project
/// </summary>
public record ListTasksQuery(
    Guid ProjectId,
    int PageNumber = 1,
    int PageSize = 100
) : IRequest<Result<ListTasksResponse>>;

public record ListTasksResponse(
    List<TaskDto> Tasks,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
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
