using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.Complete;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Requests;

public class CompleteRequestRequest
{
    public Guid RequestId { get; set; }
}

/// <summary>
/// Endpoint for completing a request
/// </summary>
public class CompleteRequestEndpoint : Endpoint<CompleteRequestRequest>
{
    private readonly IMediator _mediator;

    public CompleteRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/requests/{id}/complete");
        Roles("Manager", "TenantAdmin"); // Only managers and admins can complete
        Summary(s =>
        {
            s.Summary = "Complete a request";
            s.Description = "Completes a request (any status → Completed)";
            s.Response(200, "Request completed successfully");
            s.Response(400, "Cannot complete request or validation failed");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires Manager or TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(CompleteRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new CompleteRequestCommand(
            RequestId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Request completed successfully."
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
