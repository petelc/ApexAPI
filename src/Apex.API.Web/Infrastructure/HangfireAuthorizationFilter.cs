using Hangfire.Dashboard;

namespace Apex.API.Web.Infrastructure;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow in development
        if (httpContext.Request.Host.Host == "localhost")
            return true;

        // In production, check if user is authenticated and has admin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Administrator");
    }
}