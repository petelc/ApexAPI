using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Projects.Start;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Projects.Complete;

namespace Apex.API.Web.Endpoints.Projects;

public class CompleteProjectEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public CompleteProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/projects/{id}/complete");
        Roles("TenantAdmin", "Manager", "Project Manager");
        Summary(s =>
        {
            s.Summary = "Complete a project";
            s.Description = "Changes project status from Active to Completed";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new CompleteProjectCommand(ProjectId.From(id));

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Project completed successfully." }, ct);
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