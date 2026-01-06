using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.ChangeRequests.Deny;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class DenyChangeRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for denying a change request
/// </summary>
public class DenyChangeRequestEndpoint : Endpoint<DenyChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public DenyChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/deny");
        Roles("Manager", "TenantAdmin"); // Only TenantAdmin and Manager can deny
        Summary(s =>
        {
            s.Summary = "Deny a change request";
            s.Description = "Changes request status to Denied (requires TenantAdmin or Manager role)";
            s.Response(200, "Request denied successfully");
            s.Response(400, "Cannot deny request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin or Manager role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(DenyChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new DenyChangeRequestCommand(ChangeRequestId.From(id), req.Reason);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request denied successfully." }, ct);
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
