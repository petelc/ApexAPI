using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.Create;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// Request DTO for creating a project request
/// </summary>
public class CreateProjectRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Response DTO for project request creation
/// </summary>
public class CreateProjectRequestResponse
{
    public Guid ProjectRequestId { get; set; }  // ✅ FIXED
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for creating a new project request
/// </summary>
public class CreateProjectRequestEndpoint : Endpoint<CreateProjectRequestRequest, CreateProjectRequestResponse>
{
    private readonly IMediator _mediator;

    public CreateProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests");
        Roles("User");
        Summary(s =>
        {
            s.Summary = "Create a new project request";
            s.Description = "Creates a new project request in Draft status";
            s.Response<CreateProjectRequestResponse>(201, "Project request created successfully");
            s.Response(400, "Validation errors");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CreateProjectRequestRequest req, CancellationToken ct)
    {
        var command = new CreateProjectRequestCommand(
            req.Title,
            req.Description,
            req.Priority,
            req.DueDate);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new CreateProjectRequestResponse
            {
                ProjectRequestId = result.Value.Value,  // ✅ FIXED
                Title = req.Title,
                Status = "Draft",
                Message = "Project request created successfully."  // ✅ FIXED
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/project-requests/{result.Value.Value}";
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

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
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Errors = result.Errors
                }, ct);
            }
        }
    }
}