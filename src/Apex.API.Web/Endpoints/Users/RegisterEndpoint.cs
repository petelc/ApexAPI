using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users.Register;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Request DTO for user registration
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
}

/// <summary>
/// Response DTO for user registration
/// </summary>
public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for user registration
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private readonly IMediator _mediator;

    public RegisterEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register a new user";
            s.Description = "Creates a new user account within the current tenant";
            s.Response<RegisterResponse>(201, "User created successfully");
            s.Response(400, "Validation errors or user already exists");
        });
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            req.Email,
            req.Password,
            req.FirstName,
            req.LastName,
            req.PhoneNumber,
            req.TimeZone);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var response = new RegisterResponse
            {
                UserId = result.Value,  // ✅ FIXED: result.Value is already a Guid!
                Email = req.Email,
                FullName = $"{req.FirstName} {req.LastName}",
                Message = "User registered successfully. Please check your email to verify your account."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/users/{result.Value}";  // ✅ FIXED: Just use result.Value
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            if (result.ValidationErrors.Any())
            {
                // Return validation errors
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
                // Return general errors
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Errors = result.Errors
                }, ct);
            }
        }
    }
}