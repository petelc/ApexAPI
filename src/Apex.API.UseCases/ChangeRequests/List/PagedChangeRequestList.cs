using Apex.API.UseCases.ChangeRequests.GetById;

namespace Apex.API.UseCases.ChangeRequests.List;

public record PagedChangeRequestList(
    List<ChangeRequestDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);