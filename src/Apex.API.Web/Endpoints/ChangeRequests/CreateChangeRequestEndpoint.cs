using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.Create;

namespace Apex.API.Web.Endpoints.ChangeRequests;

/// <summary>
/// Request DTO for creating a change request
/// </summary>
public class CreateChangeRequestRequest
{
    public Guid ChangeRequestId { get; set; }  // ✅ FIXED
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string ImpactAssessment { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public string AffectedSystems { get; set; } = string.Empty;
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public string? ChangeWindow { get; set; }
}

/// <summary>
/// Response DTO for change request creation
/// </summary>
public class CreateChangeRequestResponse
{
    public Guid ChangeRequestId { get; set; }  // ✅ FIXED
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string ImpactAssessment { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public string AffectedSystems { get; set; } = string.Empty;
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public string? ChangeWindow { get; set; }
}

/// <summary>
/// Endpoint for creating a new change request
/// </summary>
public class CreateChangeRequestEndpoint : Endpoint<CreateChangeRequestRequest, CreateChangeRequestResponse>
{
    private readonly IMediator _mediator;

    public CreateChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests");
        Roles("User"); // Any authenticated user can create
        Summary(s =>
        {
            s.Summary = "Create a new change request";
            s.Description = "Creates a new change request in Draft status";
            s.Response<CreateChangeRequestResponse>(201, "Change request created successfully");
            s.Response(400, "Validation errors");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CreateChangeRequestRequest req, CancellationToken ct)
    {
        var command = new CreateChangeRequestCommand(
                req.Title,
                req.Description,
                req.ChangeType,
                req.Priority,
                req.RiskLevel,
                req.ImpactAssessment,
                req.RollbackPlan,
                req.AffectedSystems,
                req.ScheduledStartDate,
                req.ScheduledEndDate,
                req.ChangeWindow);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new CreateChangeRequestResponse
            {
                ChangeRequestId = result.Value.Value,  // ✅ FIXED
                Title = req.Title,
                Description = req.Description,
                ChangeType = req.ChangeType,
                Priority = req.Priority,
                RiskLevel = req.RiskLevel,
                ImpactAssessment = req.ImpactAssessment,
                RollbackPlan = req.RollbackPlan,
                AffectedSystems = req.AffectedSystems,
                ScheduledStartDate = req.ScheduledStartDate,
                ScheduledEndDate = req.ScheduledEndDate,
                ChangeWindow = req.ChangeWindow
            };
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/change-requests/{result.Value.Value}";
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

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
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Errors = result.Errors
                }, ct);
            }
        }
    }
}