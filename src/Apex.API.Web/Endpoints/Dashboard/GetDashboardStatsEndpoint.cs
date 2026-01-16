using Apex.API.UseCases.Dashboard.DTOs;
using Apex.API.UseCases.Common.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace Apex.API.Web.Endpoints.Dashboard;

/// <summary>
/// Get dashboard statistics
/// </summary>
public class GetDashboardStatsEndpoint : EndpointWithoutRequest<DashboardStatsDto>
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<GetDashboardStatsEndpoint> _logger;

    public GetDashboardStatsEndpoint(
        IDashboardService dashboardService,
        ILogger<GetDashboardStatsEndpoint> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/dashboard/stats");
        Description(b => b
            .WithTags("Dashboard")
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns comprehensive dashboard statistics for changes, projects, and tasks"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "User not authenticated"
                }, ct);
                return;
            }

            // Get dashboard stats
            var stats = await _dashboardService.GetDashboardStatsAsync(userId, ct);

            await HttpContext.Response.WriteAsJsonAsync(stats, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "An error occurred while retrieving dashboard statistics"
            }, ct);
        }
    }
}
