using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.UpdateNotes;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Request DTO for updating implementation notes
/// </summary>
public class UpdateImplementationNotesRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Endpoint for updating task implementation notes
/// </summary>
public class UpdateImplementationNotesEndpoint : Endpoint<UpdateImplementationNotesRequest>
{
    private readonly IMediator _mediator;

    public UpdateImplementationNotesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/tasks/{id}/implementation-notes");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Update implementation notes")
            .WithDescription("Updates notes about how the task is being implemented"));
    }

    public override async Task HandleAsync(UpdateImplementationNotesRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new UpdateImplementationNotesCommand(
            TaskId.From(taskId),
            req.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Implementation notes updated successfully." 
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
