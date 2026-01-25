using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Apex.API.Infrastructure.Data;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Get all available roles
/// </summary>
public class GetAllRolesEndpoint : EndpointWithoutRequest
{
    private readonly ApexDbContext _context;
    private readonly ILogger<GetAllRolesEndpoint> _logger;

    public GetAllRolesEndpoint(
        ApexDbContext context,
        ILogger<GetAllRolesEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/admin/roles");
        Roles("TenantAdmin");

        Description(b => b
            .WithTags("Admin")
            .WithSummary("Get all roles")
            .WithDescription("Returns a list of all available roles in the system."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Query roles directly from database
            var roles = await _context.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync(ct);

            _logger.LogInformation("Retrieved {Count} roles", roles.Count);

            await HttpContext.Response.WriteAsJsonAsync(roles, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");

            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Failed to retrieve roles"
            }, ct);
        }
    }
}
