using MediatR;
using Ardalis.Result;

namespace Apex.API.UseCases.Projects.List;

public record ListProjectsQuery(
    string? Status = null,
    string? Priority = null,
    Guid? ProjectManagerUserId = null,
    bool? IsOverdue = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedProjectList>>;