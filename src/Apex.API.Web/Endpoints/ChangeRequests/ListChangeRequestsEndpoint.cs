using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.List;

namespace Apex.API.Web.Endpoints.ChangeRequests;

/// <summary>
/// Endpoint for listing change requests with filtering
/// </summary>
public class ListChangeRequestsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListChangeRequestsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/change-requests");
        Summary(s =>
        {
            s.Summary = "List all change requests";
            s.Description = "Retrieves a paginated list of change requests with optional filtering";
            s.Response<PagedChangeRequestList>(200, "Change requests retrieved successfully");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get query parameters
        var status = Query<string>("status", isRequired: false);
        var priority = Query<string>("priority", isRequired: false);
        var assignedToUserId = Query<Guid?>("assignedToUserId", isRequired: false);
        var createdByUserId = Query<Guid?>("createdByUserId", isRequired: false);
        var isOverdue = Query<bool?>("isOverdue", isRequired: false);
        var pageNumber = Query<int?>("pageNumber", isRequired: false) ?? 1;
        var pageSize = Query<int?>("pageSize", isRequired: false) ?? 20;

        // Validate page size
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 20;

        var query = new ListChangeRequestsQuery(
            status,
            priority,
            assignedToUserId,
            createdByUserId,
            isOverdue,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
