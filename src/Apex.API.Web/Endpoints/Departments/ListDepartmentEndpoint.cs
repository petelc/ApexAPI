using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Departments.List;

namespace Apex.API.Web.Endpoints.Departments;

public class ListDepartmentsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListDepartmentsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/departments");
        Summary(s =>
        {
            s.Summary = "List all departments";
            s.Description = "Returns all departments in the tenant";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isActive = Query<bool?>("isActive", isRequired: false);

        var query = new ListDepartmentsQuery(isActive);

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