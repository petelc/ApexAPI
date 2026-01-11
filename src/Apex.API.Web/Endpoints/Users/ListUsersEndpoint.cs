using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users.List;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Endpoint for listing users in the current tenant
/// </summary>
public class ListUsersEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public ListUsersEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/users");
        Roles("Administrator", "TenantAdmin"); // All roles can list users
        Summary(s =>
        {
            s.Summary = "List all users in current tenant";
            s.Description = "Retrieves a list of all active and inactive users for assignment dropdowns, etc.";
            s.Response<List<UserDto>>(200, "Users retrieved successfully");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new ListUsersQuery();
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
