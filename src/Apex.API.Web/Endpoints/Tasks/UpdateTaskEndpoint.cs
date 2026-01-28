using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Update;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Request DTO for updating task
/// </summary>
public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public decimal? EstimatedHours { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Endpoint for updating task details
/// </summary>
public class UpdateTaskEndpoint : Endpoint<UpdateTaskRequest>
{
    private readonly IMediator _mediator;

    public UpdateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/tasks/{id}");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Update task details")
            .WithDescription("Updates title, description, priority, estimated hours, and due date"));
    }

    public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new UpdateTaskCommand(
            TaskId.From(taskId),
            req.Title,
            req.Description,
            req.Priority,
            req.EstimatedHours,
            req.DueDate);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Task updated successfully." 
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
