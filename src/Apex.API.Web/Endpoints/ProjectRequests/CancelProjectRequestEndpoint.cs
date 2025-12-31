using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.Cancel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ProjectRequests;

public class CancelProjectRequestRequest
{
    public Guid RequestId { get; set; }
}

/// <summary>
/// Endpoint for canceling a request
/// </summary>
public class CancelProjectRequestEndpoint : Endpoint<CancelProjectRequestRequest>
{
    private readonly IMediator _mediator;

    public CancelProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests/{id}/cancel");
        Roles("Manager", "TenantAdmin"); // Only managers and admins can cancel
        Summary(s =>
        {
            s.Summary = "Cancel a request";
            s.Description = "Cancels a request (any status → Canceled)";
            s.Response(200, "Request canceled successfully");
            s.Response(400, "Cannot cancel request or validation failed");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires Manager or TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(CancelProjectRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new CancelProjectRequestCommand(
            ProjectRequestId.From(id));
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Request canceled successfully."
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
