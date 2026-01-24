using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.Submit;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// Endpoint for submitting a request for review
/// </summary>
public class SubmitProjectRequestEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public SubmitProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests/{id}/submit");
        Roles("User");
        Tags("Project-Requests");
        Summary(s =>
        {
            s.Summary = "Submit a request for review";
            s.Description = "Changes request status from Draft to Pending";
            s.Response(200, "Request submitted successfully");
            s.Response(400, "Cannot submit request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires User role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new SubmitProjectRequestCommand(ProjectRequestId.From(id));

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
