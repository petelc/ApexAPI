using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.Create;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public decimal? EstimatedHours { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CreateTaskResponse
{
    public Guid TaskId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreateTaskEndpoint : Endpoint<CreateTaskRequest, CreateTaskResponse>
{
    private readonly IMediator _mediator;

    public CreateTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/projects/{projectId}/tasks");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Create a task for a project";
            s.Description = "Creates a new task within a project";
        });
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");

        var command = new CreateTaskCommand(
            ProjectId.From(projectId),
            req.Title,
            req.Description,
            req.Priority,
            req.EstimatedHours,
            req.DueDate);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new CreateTaskResponse
            {
                TaskId = result.Value.Value,
                Message = "Task created successfully."
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