using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.AssignToUser;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class AssignTaskToUserRequest
{
    public Guid AssignedToUserId { get; set; }  // ✅ Clear naming
}

public class AssignTaskToUserEndpoint : Endpoint<AssignTaskToUserRequest>
{
    private readonly IMediator _mediator;

    public AssignTaskToUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/assign-to-user");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Assign task to user";
            s.Description = "Assigns task to a specific user";
        });
    }

    public override async Task HandleAsync(AssignTaskToUserRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new AssignTaskToUserCommand(
            TaskId.From(taskId),
            req.AssignedToUserId);  // ✅ Just pass the Guid directly!

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Task assigned to user successfully." }, ct);
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