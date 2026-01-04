using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.LogTime;
using Apex.API.Core.ValueObjects;
using Serilog;

namespace Apex.API.Web.Endpoints.Tasks;

public class LogTimeRequest
{
    public decimal Hours { get; set; } = 0.0m;
}

public class LogTimeResponse
{
    public Guid TaskId { get; set; }
    public decimal TotalLoggedHours { get; set; }
}

public class LogTimeTaskEndpoint : Endpoint<LogTimeRequest, LogTimeResponse>
{
    private readonly IMediator _mediator;

    public LogTimeTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/log-time");
        Roles("TenantAdmin", "Manager", "ProjectManager", "Developer");
        Summary(s =>
        {
            s.Summary = "Log time for a task";
            s.Description = "Logs time spent on a task";
        });
    }

    public override async Task HandleAsync(LogTimeRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new LogTimeCommand(TaskId.From(id), req.Hours);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new LogTimeResponse
            {
                TaskId = id,
                TotalLoggedHours = result.Value
            };
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
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