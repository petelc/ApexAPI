using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Users.Login;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Request DTO for user login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Endpoint for user login
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/users/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Authenticate a user";
            s.Description = "Validates credentials and returns JWT access and refresh tokens";
            s.Response<LoginResponse>(200, "Login successful");
            s.Response(400, "Invalid credentials or validation errors");
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var command = new LoginCommand(req.Email, req.Password);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            // ✅ FIXED: Use correct FastEndpoints methods
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
        }
        else
        {
            // ✅ FIXED: Use correct FastEndpoints methods
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                Errors = result.Errors
            }, ct);
        }
    }
}