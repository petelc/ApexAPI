using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.UpdateNotes;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Request DTO for updating resolution notes
/// </summary>
public class UpdateResolutionNotesRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Endpoint for updating task resolution notes
/// </summary>
public class UpdateResolutionNotesEndpoint : Endpoint<UpdateResolutionNotesRequest>
{
    private readonly IMediator _mediator;

    public UpdateResolutionNotesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/tasks/{id}/resolution-notes");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Update resolution notes")
            .WithDescription("Updates notes about how the task was resolved/completed"));
    }

    public override async Task HandleAsync(UpdateResolutionNotesRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new UpdateResolutionNotesCommand(
            TaskId.From(taskId),
            req.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                Message = "Resolution notes updated successfully." 
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
