using Apex.API.UseCases.Projects.DTOs;

namespace Apex.API.UseCases.Projects.List;

public record PagedProjectList(
    List<ProjectDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);