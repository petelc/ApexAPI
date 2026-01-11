using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Projects.List;

namespace Apex.API.Web.Endpoints.Projects;

public class ListProjectsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListProjectsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/projects");
        Roles("User");
        Summary(s =>
        {
            s.Summary = "List all projects";
            s.Description = "Returns a paginated list of projects with optional filters";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var status = Query<string?>("status", isRequired: false);
        var priority = Query<string?>("priority", isRequired: false);
        var projectManagerUserId = Query<Guid?>("projectManagerUserId", isRequired: false);
        var isOverdue = Query<bool?>("isOverdue", isRequired: false);
        var pageNumber = Query<int?>("pageNumber", isRequired: false);
        var pageSize = Query<int?>("pageSize", isRequired: false);

        var query = new ListProjectsQuery(
            status,
            priority,
            projectManagerUserId,
            isOverdue,
            pageNumber ?? 1,
            Math.Min(pageSize ?? 20, 100));

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