using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.Update;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

/// <summary>
/// Request DTO for updating a request
/// </summary>
public class UpdateChangeRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ImpactAssessment { get; set; }
    public string? RollbackPlan { get; set; }
    public string? AffectedSystems { get; set; }
    public string? RiskLevel { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public string? ChangeWindow { get; set; }
}

/// <summary>
/// Endpoint for updating a request
/// </summary>
public class UpdateChangeRequestEndpoint : Endpoint<UpdateChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public UpdateChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/change-requests/{id}");
        Roles("TenantAdmin", "Change Manager", "CAB Member", "Manager", "Change Implementer");
        Summary(s =>
        {
            s.Summary = "Update a request";
            s.Description = "Updates a request (only allowed in Draft status, only by the creator)";
            s.Response(200, "Request updated successfully");
            s.Response(400, "Cannot update request or validation errors");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - can only update your own requests");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(UpdateChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new UpdateChangeRequestCommand(
            ChangeRequestId.From(id),
            req.Title,
            req.Description,
            req.ImpactAssessment ?? string.Empty,
            req.RollbackPlan ?? string.Empty,
            req.AffectedSystems ?? string.Empty,
            req.Priority,
            req.RiskLevel,
            req.ScheduledStartDate,
            req.ScheduledEndDate,
            req.ChangeWindow);

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