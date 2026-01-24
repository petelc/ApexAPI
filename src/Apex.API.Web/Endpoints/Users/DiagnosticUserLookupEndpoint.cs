using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// DIAGNOSTIC: Check if UserManager can find a specific user by ID
/// </summary>
public class DiagnosticUserLookupEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DiagnosticUserLookupEndpoint> _logger;

    public DiagnosticUserLookupEndpoint(
        UserManager<User> userManager,
        ILogger<DiagnosticUserLookupEndpoint> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/users/lookup-debug/{userId}");
        AllowAnonymous();
        
        Description(b => b
            .WithTags("Users", "Diagnostic")
            .WithSummary("DEBUG: Check if UserManager can find user by ID"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdString = Route<string>("userId");
        
        _logger.LogInformation("🔍 Looking up user: {UserId}", userIdString);

        // Try multiple approaches
        var results = new Dictionary<string, object>();

        // Approach 1: Direct string lookup (what we're currently doing)
        try
        {
            var user1 = await _userManager.FindByIdAsync(userIdString ?? string.Empty);
            results["findByIdAsync_string"] = user1 != null
                ? new
                {
                    found = true,
                    id = user1.Id,
                    email = user1.Email,
                    fullName = user1.FullName,
                    tenantId = user1.TenantId.Value
                }
                : new { found = false };
            _logger.LogInformation("  FindByIdAsync(string): {Found}", user1 != null);
        }
        catch (Exception ex)
        {
            results["findByIdAsync_string"] = new { error = ex.Message };
            _logger.LogError(ex, "  FindByIdAsync(string) threw exception");
        }

        // Approach 2: Try parsing as Guid first
        if (Guid.TryParse(userIdString, out var userGuid))
        {
            try
            {
                var user2 = await _userManager.FindByIdAsync(userGuid.ToString());
                results["findByIdAsync_guid_toString"] = user2 != null
                    ? new
                    {
                        found = true,
                        id = user2.Id,
                        email = user2.Email,
                        fullName = user2.FullName,
                        tenantId = user2.TenantId.Value
                    }
                    : new { found = false };
                _logger.LogInformation("  FindByIdAsync(Guid.ToString()): {Found}", user2 != null);
            }
            catch (Exception ex)
            {
                results["findByIdAsync_guid_toString"] = new { error = ex.Message };
                _logger.LogError(ex, "  FindByIdAsync(Guid.ToString()) threw exception");
            }

            // Approach 3: Try with dashes removed
            var noDashes = userGuid.ToString("N"); // No dashes
            try
            {
                var user3 = await _userManager.FindByIdAsync(noDashes);
                results["findByIdAsync_noDashes"] = user3 != null
                    ? new
                    {
                        found = true,
                        id = user3.Id,
                        email = user3.Email,
                        fullName = user3.FullName,
                        tenantId = user3.TenantId.Value
                    }
                    : new { found = false };
                _logger.LogInformation("  FindByIdAsync(noDashes): {Found}", user3 != null);
            }
            catch (Exception ex)
            {
                results["findByIdAsync_noDashes"] = new { error = ex.Message };
                _logger.LogError(ex, "  FindByIdAsync(noDashes) threw exception");
            }
        }
        else
        {
            results["guid_parse"] = new { error = "Could not parse userId as Guid" };
        }

        // Approach 4: Check what format the IDs are actually stored as
        var allUsers = _userManager.Users.Take(5).ToList();
        results["sample_users"] = allUsers.Select(u => new
        {
            id = u.Id,
            id_string = u.Id.ToString(),
            email = u.Email,
            tenantId = u.TenantId.Value
        }).ToList();

        await HttpContext.Response.WriteAsJsonAsync(new
        {
            requestedUserId = userIdString,
            results = results,
            recommendation = DetermineRecommendation(results)
        }, ct);
    }

    private string DetermineRecommendation(Dictionary<string, object> results)
    {
        if (results.ContainsKey("findByIdAsync_string"))
        {
            var result = results["findByIdAsync_string"] as dynamic;
            if (result?.found == true)
                return "✅ FindByIdAsync(string) works! No changes needed.";
        }

        if (results.ContainsKey("findByIdAsync_guid_toString"))
        {
            var result = results["findByIdAsync_guid_toString"] as dynamic;
            if (result?.found == true)
                return "⚠️ Need to use Guid.ToString() format";
        }

        if (results.ContainsKey("findByIdAsync_noDashes"))
        {
            var result = results["findByIdAsync_noDashes"] as dynamic;
            if (result?.found == true)
                return "⚠️ Need to use Guid without dashes";
        }

        return "❌ None of the approaches found the user. Check if user ID is correct or if user exists in database.";
    }
}
