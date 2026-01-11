using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.GetById;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

/// <summary>
/// Endpoint for getting a change request by ID
/// </summary>
public class GetChangeRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/change-requests/{id}");
        Roles("User", "Manager", "TenantAdmin", "Change Manager", "CAB Member", "Change Implementer"); // Any authenticated user 
        Summary(s =>
        {
            s.Summary = "Get a change request by ID";
            s.Description = "Retrieves detailed information about a specific change request";
            s.Response<ChangeRequestDto>(200, "Change request found");
            s.Response(404, "Change request not found");
            s.Response(403, "Forbidden - change request belongs to another tenant");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetChangeRequestByIdQuery(ChangeRequestId.From(id));

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
        }
        else if (result.Status == Ardalis.Result.ResultStatus.NotFound)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
        else if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = new[] { "Access denied" } }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
