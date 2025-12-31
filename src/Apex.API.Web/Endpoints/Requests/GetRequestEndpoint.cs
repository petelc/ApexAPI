using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.GetById;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Requests;

/// <summary>
/// Endpoint for getting a request by ID
/// </summary>
public class GetRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/requests/{id}");
        Summary(s =>
        {
            s.Summary = "Get a request by ID";
            s.Description = "Retrieves detailed information about a specific request";
            s.Response<RequestDto>(200, "Request found");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - request belongs to another tenant");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetRequestByIdQuery(RequestId.From(id));

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
