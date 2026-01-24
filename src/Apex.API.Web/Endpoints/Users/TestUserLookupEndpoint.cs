using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;

namespace Apex.API.Web.Endpoints.Users;

public class TestUserLookupEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;

    public TestUserLookupEndpoint(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Get("/test-user-lookup/{userId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<string>("userId") ?? "";
        
        // Try direct lookup
        var user = await _userManager.FindByIdAsync(userId);
        
        // Also get a few sample users to see ID format
        var samples = _userManager.Users.Take(3).ToList();
        
        await HttpContext.Response.WriteAsJsonAsync(new
        {
            searchedFor = userId,
            found = user != null,
            userDetails = user != null ? new
            {
                id = user.Id.ToString(),
                email = user.Email,
                fullName = user.FullName,
                tenantId = user.TenantId.Value.ToString()
            } : null,
            sampleUsers = samples.Select(u => new
            {
                id = u.Id.ToString(),
                email = u.Email
            }).ToList()
        }, ct);
    }
}
