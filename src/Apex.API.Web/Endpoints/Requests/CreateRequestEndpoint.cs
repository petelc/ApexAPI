using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Requests.Create;

namespace Apex.API.Web.Endpoints.Requests;

/// <summary>
/// Request DTO for creating a request
/// </summary>
public class CreateRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Response DTO for request creation
/// </summary>
public class CreateRequestResponse
{
    public Guid RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for creating a new request
/// </summary>
public class CreateRequestEndpoint : Endpoint<CreateRequestRequest, CreateRequestResponse>
{
    private readonly IMediator _mediator;

    public CreateRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/requests");
        // Authenticated users only (no AllowAnonymous)
        Summary(s =>
        {
            s.Summary = "Create a new request";
            s.Description = "Creates a new request in Draft status";
            s.Response<CreateRequestResponse>(201, "Request created successfully");
            s.Response(400, "Validation errors");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CreateRequestRequest req, CancellationToken ct)
    {
        var command = new CreateRequestCommand(
            req.Title,
            req.Description,
            req.Priority,
            req.DueDate);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new CreateRequestResponse
            {
                RequestId = result.Value.Value,
                Title = req.Title,
                Status = "Draft",
                Message = "Request created successfully."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/requests/{result.Value.Value}";
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
