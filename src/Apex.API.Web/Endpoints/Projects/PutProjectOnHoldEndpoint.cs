using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.Projects.PutOnHold;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Endpoint to put a project on hold
/// </summary>
[HttpPost("/projects/{projectId}/hold")]
[Authorize]
public class PutProjectOnHoldEndpoint : Endpoint<PutProjectOnHoldRequest, PutProjectOnHoldResponse>
{
    private readonly IMediator _mediator;

    public PutProjectOnHoldEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(PutProjectOnHoldRequest req, CancellationToken ct)
    {
        var command = new PutProjectOnHoldCommand(
            ProjectId.From(req.ProjectId),
            req.Reason
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = "Project put on hold successfully."
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

public class PutProjectOnHoldRequest
{
    public Guid ProjectId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PutProjectOnHoldResponse
{
    public string Message { get; set; } = string.Empty;
}
