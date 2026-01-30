using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Checklist;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Endpoint for toggling checklist item completion
/// </summary>
public class ToggleChecklistItemEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ToggleChecklistItemEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/tasks/{taskId}/checklist/{itemId}/toggle");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Toggle checklist item")
            .WithDescription("Marks checklist item as completed or incomplete"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var itemId = Route<Guid>("itemId");

        var command = new ToggleChecklistItemCommand(
            TaskChecklistItemId.From(itemId));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Checklist item toggled successfully." 
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
