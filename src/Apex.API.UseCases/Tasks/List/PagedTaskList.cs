using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.List;

public record PagedTaskList(
    List<TaskDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);