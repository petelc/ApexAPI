using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Projects.AssignProjectManager;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to assign a project manager to a project
/// </summary>
public class AssignProjectManagerEndpoint : Endpoint<AssignProjectManagerRequest>
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
        
        Description(b => b
            .WithTags("Projects")
            .WithSummary("Assign a project manager to a project")
            .WithDescription("Assigns a specified user as the project manager for the given project."));
    }

    public override async Task HandleAsync(AssignProjectManagerRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        
        var command = new AssignProjectManagerCommand(
            ProjectId.From(projectId),
            req.ProjectManagerUserId
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // ✅ Use HttpContext.Response pattern (same as ListUsersEndpoint)
            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        else
        {
            // Handle different error statuses
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { errors = result.Errors }, ct);
                return;
            }

            if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await HttpContext.Response.WriteAsJsonAsync(new { errors = result.Errors }, ct);
                return;
            }

            // Default to 400 Bad Request
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { errors = result.Errors }, ct);
        }
    }
}

/// <summary>
/// Request model for assigning project manager
/// ✅ FIXED: Field name matches what frontend sends
/// </summary>
public class AssignProjectManagerRequest
{
    public Guid ProjectManagerUserId { get; set; }
}
