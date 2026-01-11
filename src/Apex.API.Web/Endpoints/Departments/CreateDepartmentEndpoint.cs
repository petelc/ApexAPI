using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Departments.Create;

namespace Apex.API.Web.Endpoints.Departments;

public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? DepartmentManagerUserId { get; set; }
}

public class CreateDepartmentResponse
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CreateDepartmentEndpoint : Endpoint<CreateDepartmentRequest, CreateDepartmentResponse>
{
    private readonly IMediator _mediator;

    public CreateDepartmentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/departments");
        Roles("TenantAdmin", "Administrator");
        Summary(s =>
        {
            s.Summary = "Create a new department";
            s.Description = "Creates a new organizational department (e.g., Infrastructure, Security, Development)";
        });
    }

    public override async Task HandleAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        var command = new CreateDepartmentCommand(
            req.Name,
            req.Description,
            req.DepartmentManagerUserId);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new CreateDepartmentResponse
            {
                DepartmentId = result.Value.Value,
                Name = req.Name,
                Message = "Department created successfully."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
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
                        ErrorMessage = e.ErrorMessage
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