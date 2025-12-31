using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.Submit;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Requests;

/// <summary>
/// Endpoint for submitting a request for review
/// </summary>
public class SubmitRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public SubmitRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/requests/{id}/submit");
        Summary(s =>
        {
            s.Summary = "Submit a request for review";
            s.Description = "Changes request status from Draft to Pending";
            s.Response(200, "Request submitted successfully");
            s.Response(400, "Cannot submit request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new SubmitRequestCommand(RequestId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request submitted successfully." }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
