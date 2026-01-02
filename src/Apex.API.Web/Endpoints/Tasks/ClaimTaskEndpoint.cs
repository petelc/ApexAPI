using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.Claim;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

///<summary>
/// Endpoint for users to claim department-assigned tasks
///</summary>
public class ClaimTaskEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ClaimTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/claim");
        Summary(s =>
        {
            s.Summary = "Claim a department task";
            s.Description = "Allows a department member to claim a task assigned to their department";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new ClaimTaskCommand(TaskId.From(taskId));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Task claimed successfully." }, ct);
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