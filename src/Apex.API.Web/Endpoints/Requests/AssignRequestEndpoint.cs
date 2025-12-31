using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.Assign;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Requests;

public class AssignRequestRequest
{
    public Guid AssignedToUserId { get; set; }
}

/// <summary>
/// Endpoint for assigning a request to a user
/// </summary>
public class AssignRequestEndpoint : Endpoint<AssignRequestRequest>
{
    private readonly IMediator _mediator;

    public AssignRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/requests/{id}/assign");
        Roles("Manager", "TenantAdmin"); // Only managers and admins can assign
        Summary(s =>
        {
            s.Summary = "Assign a request to a user";
            s.Description = "Assigns an approved request to a user (Approved → InProgress)";
            s.Response(200, "Request assigned successfully");
            s.Response(400, "Cannot assign request or user validation failed");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires Manager or TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(AssignRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new AssignRequestCommand(
            RequestId.From(id),
            req.AssignedToUserId);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Request assigned successfully.",
                AssignedToUserId = req.AssignedToUserId
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
