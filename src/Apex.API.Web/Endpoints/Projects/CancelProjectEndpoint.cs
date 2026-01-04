using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.Projects.Cancel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to cancel a project
/// </summary>
[HttpPost("/api/projects/{projectId}/cancel")]
[Authorize]
public class CancelProjectEndpoint : Endpoint<CancelProjectRequest, CancelProjectResponse>
{
    private readonly IMediator _mediator;

    public CancelProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancelProjectRequest req, CancellationToken ct)
    {
        var command = new CancelProjectCommand(
            ProjectId.From(req.ProjectId),
            req.Reason
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Project cancelled successfully." }, ct);
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

public class CancelProjectRequest
{
    public Guid ProjectId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CancelProjectResponse
{
    public string Message { get; set; } = string.Empty;
}
