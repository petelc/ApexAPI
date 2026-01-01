using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Projects.ConvertFromProjectRequest;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Projects;

public class ConvertProjectRequestRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Guid? ProjectManagerUserId { get; set; }
}

public class ConvertProjectRequestResponse
{
    public Guid ProjectId { get; set; }
    public string Message { get; set; } = string.Empty;
}

///<summary>
/// Endpoint to convert an approved ProjectRequest into a Project
///</summary>
public class ConvertProjectRequestEndpoint : Endpoint<ConvertProjectRequestRequest, ConvertProjectRequestResponse>
{
    private readonly IMediator _mediator;

    public ConvertProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests/{id}/convert");
        Roles("TenantAdmin", "Manager");
        Summary(s =>
        {
            s.Summary = "Convert approved project request to project";
            s.Description = "Creates a new Project from an approved ProjectRequest";
            s.Response<ConvertProjectRequestResponse>(201, "Project created successfully");
            s.Response(400, "ProjectRequest not in approved status");
            s.Response(403, "Forbidden - requires TenantAdmin or Manager role");
            s.Response(404, "ProjectRequest not found");
        });
    }

    public override async Task HandleAsync(ConvertProjectRequestRequest req, CancellationToken ct)
    {
        var projectRequestId = Route<Guid>("id");

        var command = new ConvertProjectRequestToProjectCommand(
            ProjectRequestId.From(projectRequestId),
            req.StartDate,
            req.EndDate,
            req.Budget,
            req.ProjectManagerUserId);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new ConvertProjectRequestResponse
            {
                ProjectId = result.Value.Value,
                Message = "Project created successfully from project request."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/projects/{result.Value.Value}";
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