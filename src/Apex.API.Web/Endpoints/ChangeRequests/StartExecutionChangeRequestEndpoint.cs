using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.StartExecution;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class StartExecutionChangeRequestRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Endpoint for starting execution of a request
/// </summary>
public class StartExecutionChangeRequestEndpoint : Endpoint<StartExecutionChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public StartExecutionChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/start-execution");
        Roles("TenantAdmin", "Change Manager", "CAB Member", "Manager"); // Only TenantAdmin can start execution
        Summary(s =>
        {
            s.Summary = "Start execution of a change request";
            s.Description = "Changes request status to In Execution (requires TenantAdmin role)";
            s.Response(200, "Request started execution successfully");
            s.Response(400, "Cannot start execution of request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(StartExecutionChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new StartExecutionChangeRequestCommand(ChangeRequestId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request started execution successfully." }, ct);
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
