using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users.List;
using Apex.API.UseCases.Users.PMs;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Endpoint for listing project managers in the current tenant
/// </summary>
public class ListPMsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListPMsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/users/project-managers");
        Roles("User"); // All roles can list project managers
        Summary(s =>
        {
            s.Summary = "List all project managers in current tenant";
            s.Description = "Retrieves a list of all active and inactive project managers for assignment dropdowns, etc.";
            s.Response<List<PMDto>>(200, "Project managers retrieved successfully");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new ListPMsQuery();
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
