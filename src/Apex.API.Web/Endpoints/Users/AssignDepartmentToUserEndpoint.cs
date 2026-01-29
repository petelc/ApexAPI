using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users.AssignDepartment;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Users;

public class AssignDepartmentRequest
{
    public Guid? DepartmentId { get; set; }  // Nullable - allows removing department
}

public class AssignDepartmentToUserEndpoint : Endpoint<AssignDepartmentRequest>
{
    private readonly IMediator _mediator;

    public AssignDepartmentToUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/users/{userId}/assign-department");
        Roles("TenantAdmin", "Administrator", "Manager");
        Summary(s =>
        {
            s.Summary = "Assign department to user";
            s.Description = "Assigns a user to a department or removes department assignment (pass null)";
        });
    }

    public override async Task HandleAsync(AssignDepartmentRequest req, CancellationToken ct)
    {
        var userId = Route<Guid>("userId");

        // ✅ FIX: Cast null to DepartmentId? to satisfy type system
        DepartmentId? departmentId = req.DepartmentId.HasValue
            ? DepartmentId.From(req.DepartmentId.Value)
            : (DepartmentId?)null;

        var command = new AssignDepartmentToUserCommand(userId, departmentId);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Message = req.DepartmentId.HasValue
                    ? "Department assigned successfully."
                    : "Department removed successfully."
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

            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}