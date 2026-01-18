using MediatR;
using Ardalis.Result;
using Apex.API.UseCases.Projects.DTOs;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Microsoft.Extensions.Logging;
using Apex.API.UseCases.Projects.Specifications;

namespace Apex.API.UseCases.Projects.List;

public record ListProjectsQuery(
    string? Status = null,
    string? Priority = null,
    Guid? ProjectManagerUserId = null,
    Guid? CreatedByUserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<ListProjectsResponse>>;

public record ListProjectsResponse(
    List<ProjectDto> Projects,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);


