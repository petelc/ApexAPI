using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Unblock;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Endpoint for unblocking a task
/// </summary>
public class UnblockTaskEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public UnblockTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/unblock");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Unblock a task")
            .WithDescription("Changes task status from Blocked back to InProgress"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new UnblockTaskCommand(TaskId.From(taskId));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Task unblocked successfully." 
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

            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
