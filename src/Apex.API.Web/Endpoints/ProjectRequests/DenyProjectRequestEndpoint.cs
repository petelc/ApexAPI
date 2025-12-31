using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.ProjectRequests.Deny;

namespace Apex.API.Web.Endpoints.ProjectRequests;

public class DenyProjectRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for denying a request
/// </summary>
public class DenyProjectRequestEndpoint : Endpoint<DenyProjectRequestRequest>
{
    private readonly IMediator _mediator;

    public DenyProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests/{id}/deny");
        Roles("Manager", "TenantAdmin"); // Only TenantAdmin and Manager can deny
        Summary(s =>
        {
            s.Summary = "Deny a request";
            s.Description = "Changes request status to Denied (requires TenantAdmin or Manager role)";
            s.Response(200, "Request denied successfully");
            s.Response(400, "Cannot deny request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin or Manager role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(DenyProjectRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new DenyProjectRequestCommand(ProjectRequestId.From(id), req.Reason);

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
