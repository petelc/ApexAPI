using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.GetById;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class GetTaskByIdEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public GetTaskByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/tasks/{id}");
        Roles("TenantAdmin", "Manager", "Project Manager", "Developer", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Get task by ID";
            s.Description = "Retrieves a task with all details";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetTaskByIdQuery(TaskId.From(id));

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