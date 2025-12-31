using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.GetById;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// Endpoint for getting a request by ID
/// </summary>
public class GetProjectRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/project-requests/{id}");
        Summary(s =>
        {
            s.Summary = "Get a request by ID";
            s.Description = "Retrieves detailed information about a specific request";
            s.Response<ProjectRequestDto>(200, "Request found");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - request belongs to another tenant");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetProjectRequestByIdQuery(ProjectRequestId.From(id));

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
