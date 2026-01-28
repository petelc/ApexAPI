using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Checklist;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Endpoint for getting task checklist
/// </summary>
public class GetTaskChecklistEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetTaskChecklistEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/tasks/{id}/checklist");
        AllowAnonymous(); // Or add your auth policy
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Get task checklist")
            .WithDescription("Returns all checklist items for a task, ordered by Order property"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var query = new GetTaskChecklistQuery(TaskId.From(taskId));

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
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
