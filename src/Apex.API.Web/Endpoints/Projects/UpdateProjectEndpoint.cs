using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.Projects.Update;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to update project details
/// </summary>
[HttpPut("/projects/{projectId}")]
[Authorize]
public class UpdateProjectEndpoint : Endpoint<UpdateProjectRequest, UpdateProjectResponse>
{
    private readonly IMediator _mediator;

    public UpdateProjectEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        var command = new UpdateProjectCommand(
            ProjectId.From(req.ProjectId),
            req.Name,
            req.Description,
            req.Budget,
            req.StartDate,
            req.EndDate,
            req.Priority
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Project updated successfully."
            }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            if (result.ValidationErrors.Any())
            {
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Errors = result.ValidationErrors.Select(e => new
                    {
                        Identifier = e.Identifier,
                        ErrorMessage = e.ErrorMessage,
                        Severity = e.Severity.ToString()
                    })
                }, ct);
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            }
        }
    }
}

public class UpdateProjectRequest
{
    public Guid ProjectId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Priority { get; set; }
}

public class UpdateProjectResponse
{
    public string Message { get; set; } = string.Empty;
}
