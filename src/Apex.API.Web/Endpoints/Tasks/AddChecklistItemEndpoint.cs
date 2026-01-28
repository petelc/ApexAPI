using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Checklist;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Request DTO for adding checklist item
/// </summary>
public class AddChecklistItemRequest
{
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>
/// Response DTO for checklist item
/// </summary>
public class ChecklistItemResponse
{
    public Guid ItemId { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for adding a checklist item to a task
/// </summary>
public class AddChecklistItemEndpoint : Endpoint<AddChecklistItemRequest, ChecklistItemResponse>
{
    private readonly IMediator _mediator;

    public AddChecklistItemEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/checklist");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Add checklist item")
            .WithDescription("Adds a to-do item to the task checklist"));
    }

    public override async Task HandleAsync(AddChecklistItemRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new AddChecklistItemCommand(
            TaskId.From(taskId),
            req.Description,
            req.Order);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new ChecklistItemResponse
            {
                ItemId = result.Value.Value,
                Message = "Checklist item added successfully."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
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
