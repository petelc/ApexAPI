using MediatR;
using Ardalis.Result;
using Apex.API.UseCases.ProjectRequests.GetById;

namespace Apex.API.UseCases.ProjectRequests.List;

/// <summary>
/// Query to list requests with filtering and pagination
/// </summary>
public record ListRequestsQuery(
    string? Status = null,
    string? Priority = null,
    Guid? AssignedToUserId = null,
    Guid? CreatedByUserId = null,
    bool? IsOverdue = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedRequestList>>;

/// <summary>
/// Paged list of requests
/// </summary>
public record PagedRequestList(
    List<ProjectRequestDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
