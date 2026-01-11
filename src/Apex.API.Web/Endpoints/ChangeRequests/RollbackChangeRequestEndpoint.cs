using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.Rollback;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class RollbackChangeRequestRequest
{
    public string Reason { get; set; } = string.Empty;
    public Guid ChangeRequestId { get; set; }
}

/// <summary>
/// Endpoint for rolling back a change request
/// </summary>
public class RollbackChangeRequestEndpoint : Endpoint<RollbackChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public RollbackChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/rollback");
        Roles("Manager", "TenantAdmin", "Change Manager", "CAB Member"); // Only managers and admins can rollback
        Summary(s =>
        {
            s.Summary = "Rollback a change request";
            s.Description = "Rolls back a change request (any status → RolledBack)";
            s.Response(200, "Change request rolled back successfully");
            s.Response(400, "Cannot rollback change request or validation failed");
            s.Response(404, "Change request not found");
            s.Response(403, "Forbidden - requires Manager, TenantAdmin, Change Manager, or CAB Member role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(RollbackChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new RollbackChangeRequestCommand(
            ChangeRequestId.From(id), Reason: req.Reason);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Request rolled back successfully."
            }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            if (result.ValidationErrors.Any())
            {
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Errors = result.ValidationErrors.Select(e => new
                    {
                        Identifier = e.Identifier,
                        ErrorMessage = e.ErrorMessage,
                        Severity = e.Severity.ToString()
                    })
                }, ct);
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            }
        }
    }
}
