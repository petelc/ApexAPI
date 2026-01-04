using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.Block;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class BlockTaskRequest
{
    public string BlockedReason { get; set; } = string.Empty;
}

public class BlockTaskResponse
{
    public Guid TaskId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BlockTaskEndpoint : Endpoint<BlockTaskRequest, BlockTaskResponse>
{
    private readonly IMediator _mediator;

    public BlockTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/block");
        Roles("TenantAdmin", "Manager", "ProjectManager", "Developer");
        Summary(s =>
        {
            s.Summary = "Block a task";
            s.Description = "Changes task status from Active to Blocked";
        });
    }

    public override async Task HandleAsync(BlockTaskRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new BlockTaskCommand(TaskId.From(id), req.BlockedReason);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new BlockTaskResponse
            {
                TaskId = id,
                Message = "Task blocked successfully."
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