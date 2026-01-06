using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ChangeRequests.Schedule;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ChangeRequests;

public class ScheduleChangeRequestRequest
{
    public DateTime ScheduleStartDate { get; set; }
    public DateTime ScheduleEndDate { get; set; }
    public string? Window { get; set; }
}

/// <summary>
/// Endpoint for scheduling a request
/// </summary>
public class ScheduleChangeRequestEndpoint : Endpoint<ScheduleChangeRequestRequest>
{
    private readonly IMediator _mediator;

    public ScheduleChangeRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/change-requests/{id}/schedule");
        Roles("TenantAdmin"); // Only TenantAdmin can schedule
        Summary(s =>
        {
            s.Summary = "Schedule a change request";
            s.Description = "Changes request status to Scheduled (requires TenantAdmin role)";
            s.Response(200, "Request scheduled successfully");
            s.Response(400, "Cannot schedule request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(ScheduleChangeRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new ScheduleChangeRequestCommand(
            ScheduleStartDate: req.ScheduleStartDate,
            ScheduleEndDate: req.ScheduleEndDate,
            Window: req.Window ?? "Default Window",
            ChangeRequestId: ChangeRequestId.From(id)
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request scheduled successfully." }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
