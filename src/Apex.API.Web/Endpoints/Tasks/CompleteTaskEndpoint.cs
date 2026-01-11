using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.Complete;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class CompleteTaskEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public CompleteTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/complete");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Complete a task";
            s.Description = "Changes task status from Active to Completed";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new CompleteTaskCommand(TaskId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Task completed successfully." }, ct);
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