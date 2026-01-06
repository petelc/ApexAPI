using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.StartReview;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class StartReviewChangeRequestRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Endpoint for starting review of a request
/// </summary>
public class StartReviewChangeRequestEndpoint : Endpoint<StartReviewChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public StartReviewChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/start-review");
        Roles("TenantAdmin"); // Only TenantAdmin can start review
        Summary(s =>
        {
            s.Summary = "Start review of a change request";
            s.Description = "Changes request status to In Review (requires TenantAdmin role)";
            s.Response(200, "Request started review successfully");
            s.Response(400, "Cannot start review of request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(StartReviewChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new StartReviewChangeRequestCommand(ChangeRequestId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request started review successfully." }, ct);
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
