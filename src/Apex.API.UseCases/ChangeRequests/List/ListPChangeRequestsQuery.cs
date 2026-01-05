using MediatR;
using Ardalis.Result;
using Apex.API.UseCases.ChangeRequests.GetById;

namespace Apex.API.UseCases.ChangeRequests.List;

/// <summary>
/// Query to list requests with filtering and pagination
/// </summary>
public record ListChangeRequestsQuery(
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
    List<ChangeRequestDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
