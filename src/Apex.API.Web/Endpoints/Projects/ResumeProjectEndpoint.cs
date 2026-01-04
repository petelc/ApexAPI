using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.Projects.Resume;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to resume a project from on-hold status
/// </summary>
[HttpPost("/api/projects/{projectId}/resume")]
[Authorize]
public class ResumeProjectEndpoint : Endpoint<ResumeProjectRequest, ResumeProjectResponse>
{
    private readonly IMediator _mediator;

    public ResumeProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(ResumeProjectRequest req, CancellationToken ct)
    {
        var command = new ResumeProjectCommand(
            ProjectId.From(req.ProjectId)
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Project resumed successfully." }, ct);
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

public class ResumeProjectRequest
{
    public Guid ProjectId { get; set; }
}

public class ResumeProjectResponse
{
    public string Message { get; set; } = string.Empty;
}
