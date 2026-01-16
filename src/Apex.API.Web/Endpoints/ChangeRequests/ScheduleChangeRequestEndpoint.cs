using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.Infrastructure.Data;
using Apex.API.UseCases.ChangeRequests.Schedule;
using FastEndpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class ScheduleChangeRequestRequest
{
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public string? Window { get; set; }
}
/// <summary>
/// Schedule a change request for implementation
/// </summary>
public class ScheduleChangeRequestEndpoint : Endpoint<ScheduleChangeRequestRequest>
{
    private readonly IMediator _mediator;  // ✅ Only need MediatR

    public ScheduleChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/schedule");
        Roles("TenantAdmin", "Change Manager", "CAB Member", "Manager", "Change Implementer");
        Summary(s =>
        {
            s.Summary = "Schedule a change request";
            s.Description = "Schedules a change request for implementation";
            s.Response(200, "Request scheduled successfully");
            s.Response(400, "Cannot schedule request or validation errors");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - can only schedule your own requests");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(ScheduleChangeRequestRequest req, CancellationToken ct)
    {
        var changeRequestId = Route<Guid>("id");

        // Validate dates
        var startDate = req.ScheduledStartDate.ToUniversalTime();
        var endDate = req.ScheduledEndDate.ToUniversalTime();

        if (startDate >= endDate)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { "Start date must be before end date" }
            }, ct);
            return;
        }

        // Create command
        var command = new ScheduleChangeRequestCommand(
            startDate,
            endDate,
            req.Window ?? string.Empty,
            ChangeRequestId.From(changeRequestId));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // ✅ Success
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                message = "Change request scheduled successfully."
            }, ct);
        }
        else
        {
            // ❌ Error
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors
            }, ct);
        }
    }
}

