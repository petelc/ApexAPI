using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.MarkFailed;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class MarkFailedChangeRequestRequest
{
    public string Reason { get; set; } = string.Empty;
    public Guid ChangeRequestId { get; set; }
}

/// <summary>
/// Endpoint for marking a change request as failed
/// </summary>
public class MarkFailedChangeRequestEndpoint : Endpoint<MarkFailedChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public MarkFailedChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/mark-failed");
        Roles("Manager", "TenantAdmin"); // Only managers and admins can mark as failed
        Summary(s =>
        {
            s.Summary = "Mark a change request as failed";
            s.Description = "Marks a change request as failed (any status → Failed)";
            s.Response(200, "Change request marked as failed successfully");
            s.Response(400, "Cannot mark change request as failed or validation failed");
            s.Response(404, "Change request not found");
            s.Response(403, "Forbidden - requires Manager or TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(MarkFailedChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new MarkFailedChangeRequestCommand(Reason: req.Reason,
            ChangeRequestId.From(id));
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Request marked as failed successfully."
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
