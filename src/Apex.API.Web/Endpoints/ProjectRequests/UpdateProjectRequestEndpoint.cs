using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.Update;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// Request DTO for updating a request
/// </summary>
public class UpdateProjectRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BusinessJustification { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public DateTime? ProposedStartDate { get; set; }
    public DateTime? ProposedEndDate { get; set; }
}

/// <summary>
/// Endpoint for updating a request
/// </summary>
public class UpdateProjectRequestEndpoint : Endpoint<UpdateProjectRequestRequest>
{
    private readonly IMediator _mediator;

    public UpdateProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/project-requests/{id}");
        Roles("User");
        Tags("Project-Requests");
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

    public override async Task HandleAsync(UpdateProjectRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new UpdateProjectRequestCommand(
            ProjectRequestId.From(id),
            req.Title,
            req.Description,
            req.BusinessJustification,
            req.Priority,
            req.DueDate,
            req.EstimatedBudget,
            req.ProposedStartDate,
            req.ProposedEndDate);

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