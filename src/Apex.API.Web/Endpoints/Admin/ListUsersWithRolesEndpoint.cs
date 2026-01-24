using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Get all users in current tenant with their roles
/// </summary>
public class ListUsersWithRolesEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;

    public ListUsersWithRolesEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public override void Configure()
    {
        Get("/admin/users");
        Roles("TenantAdmin");
        
        Description(b => b
            .WithTags("Admin")
            .WithSummary("List all users with their roles")
            .WithDescription("Returns all users in current tenant with their assigned roles."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get all users in current tenant
        var users = await _userManager.Users
            .Where(u => u.TenantId == _tenantContext.CurrentTenantId)
            .ToListAsync(ct);

        // Get roles for each user
        var usersWithRoles = new List<UserWithRolesDto>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            usersWithRoles.Add(new UserWithRolesDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            });
        }

        await HttpContext.Response.WriteAsJsonAsync(usersWithRoles, ct);
    }
}

public class UserWithRolesDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
}
