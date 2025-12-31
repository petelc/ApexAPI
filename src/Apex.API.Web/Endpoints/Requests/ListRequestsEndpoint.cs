using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.List;

namespace Apex.API.Web.Endpoints.Requests;

/// <summary>
/// Endpoint for listing requests with filtering
/// </summary>
public class ListRequestsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListRequestsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/requests");
        Summary(s =>
        {
            s.Summary = "List all requests";
            s.Description = "Retrieves a paginated list of requests with optional filtering";
            s.Response<PagedRequestList>(200, "Requests retrieved successfully");
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

        var query = new ListRequestsQuery(
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
