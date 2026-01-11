using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.UpdateNotes;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

/// <summary>
/// Request DTO for updating a request's notes
/// </summary>
public class UpdateNotesChangeRequestRequest
{
    public string ImplementationNotes { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for updating a request's notes
/// </summary>
public class UpdateNotesChangeRequestEndpoint : Endpoint<UpdateNotesChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public UpdateNotesChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/change-requests/{id}/update-notes");
        Roles("TenantAdmin", "Change Manager", "CAB Member", "Manager", "Change Implementer");
        Summary(s =>
        {
            s.Summary = "Update a request's notes";
            s.Description = "Updates a request's notes (only allowed in Draft status, only by the creator)";
            s.Response(200, "Request updated successfully");
            s.Response(400, "Cannot update request or validation errors");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - can only update your own requests");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(UpdateNotesChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new UpdateNotesChangeRequestCommand(
            ChangeRequestId.From(id),
            req.ImplementationNotes);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Request updated successfully."
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