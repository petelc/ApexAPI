using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Tasks.List;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

public class ListTasksEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListTasksEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/projects/{id}/tasks");
        Roles("TenantAdmin", "Manager", "Project Manager", "Developer", "Change Implementer", "Change Manager", "CAB Member", "CAB Manager");
        Summary(s =>
        {
            s.Summary = "List tasks";
            s.Description = "Retrieves tasks";
        });
    }
    Working on adding user lookup
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var pageNumber = Query<int?>("pageNumber", isRequired: false);
        var pageSize = Query<int?>("pageSize", isRequired: false);
        var query = new ListTasksQuery(
            id,
            pageNumber ?? 1,
            pageSize ?? 100);

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
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