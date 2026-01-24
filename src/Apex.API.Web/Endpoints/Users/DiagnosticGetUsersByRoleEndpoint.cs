using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// DIAGNOSTIC: Get users by role with detailed logging
/// Use this to debug why GetUsersByRoleEndpoint returns nothing
/// </summary>
public class DiagnosticGetUsersByRoleEndpoint : Endpoint<GetUsersByRoleRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DiagnosticGetUsersByRoleEndpoint> _logger;

    public DiagnosticGetUsersByRoleEndpoint(
        UserManager<User> userManager,
        ILogger<DiagnosticGetUsersByRoleEndpoint> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/users/by-role-debug/{role}");
        AllowAnonymous();
        
        Description(b => b
            .WithTags("Users", "Diagnostic")
            .WithSummary("DEBUG: Get users by role with detailed logging"));
    }

    public override async Task HandleAsync(GetUsersByRoleRequest req, CancellationToken ct)
    {
        _logger.LogInformation("🔍 DIAGNOSTIC: Starting GetUsersByRole for role: {Role}", req.Role);

        // Get users in role
        var usersInRole = await _userManager.GetUsersInRoleAsync(req.Role);
        _logger.LogInformation("👥 Users found in role '{Role}': {Count}", req.Role, usersInRole.Count);

        if (usersInRole.Count == 0)
        {
            _logger.LogWarning("⚠️ No users found in role '{Role}'", req.Role);
        }

        // Log each user with their tenant
        foreach (var user in usersInRole)
        {
            _logger.LogInformation("  - User: {Id} | {FullName} | {Email} | Tenant: {TenantId}", 
                user.Id, user.FullName, user.Email, user.TenantId.Value);
        }

        // Map to DTOs with tenant info
        var userDtos = usersInRole.Select(u => new
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            TenantId = u.TenantId.Value
        }).ToList();

        _logger.LogInformation("✅ Returning {Count} users", userDtos.Count);

        await HttpContext.Response.WriteAsJsonAsync(new
        {
            role = req.Role,
            count = userDtos.Count,
            users = userDtos
        }, ct);
    }
}
