using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class AssignTaskToDepartmentRequest
{
    public Guid DepartmentId { get; set; }
}

public class AssignTaskToDepartmentEndpoint : Endpoint<AssignTaskToDepartmentRequest>
{
    private readonly IMediator _mediator;

    public AssignTaskToDepartmentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/tasks/{id}/assign-to-department");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Assign task to department";
            s.Description = "Assigns task to a department - any department member can claim it";
        });
    }

    public override async Task HandleAsync(AssignTaskToDepartmentRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var command = new AssignTaskToDepartmentCommand(
            TaskId.From(taskId),
            DepartmentId.From(req.DepartmentId));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Task assigned to department successfully." }, ct);
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