using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.Projects.AssignProjectManager;
using Apex.API.Core.ValueObjects;


namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to assign a project manager to a project
/// </summary>
public class AssignProjectManagerEndpoint : Endpoint<AssignProjectManagerRequest, AssignProjectManagerResponse>
{
    private readonly IMediator _mediator;

    public AssignProjectManagerEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/projects/{projectId}/assign-pm");
        Roles("TenantAdmin", "Manager", "Project Manager", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "Assign a project manager to a project";
            s.Description = "Assigns a specified user as the project manager for the given project.";
        });
    }

    public override async Task HandleAsync(AssignProjectManagerRequest req, CancellationToken ct)
    {
        var command = new AssignProjectManagerCommand(
            ProjectId.From(req.ProjectId),
            req.ProjectManagerUserId
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Project manager assigned successfully." }, ct);
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

public class AssignProjectManagerRequest
{
    public Guid ProjectId { get; set; }
    public Guid ProjectManagerUserId { get; set; }
}

public class AssignProjectManagerResponse
{
    public string Message { get; set; } = string.Empty;
}
