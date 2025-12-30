using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data;

/// <summary>
/// Seeds initial data including system roles
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseSeeder"); // ✅ FIXED

        logger.LogInformation("Starting database seeding...");

        await SeedRolesAsync(roleManager, logger);

        logger.LogInformation("Database seeding completed.");
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        // System roles to seed
        var systemRoles = new[]
        {
            (Role.SystemRoles.TenantAdmin, "Administrator with full access to tenant", true),
            (Role.SystemRoles.User, "Standard user with basic access", true),
            (Role.SystemRoles.Manager, "Manager with elevated privileges", true),
            (Role.SystemRoles.ReadOnly, "Read-only access user", true)
        };

        foreach (var (roleName, description, isSystemRole) in systemRoles)
        {
            // Check if role exists (check by normalized name to handle any tenant)
            var existingRole = await roleManager.FindByNameAsync(roleName);

            if (existingRole == null)
            {
                // Create role for "system" tenant (shared roles)
                var role = Role.Create(
                    TenantId.From(Guid.Empty), // System tenant ID
                    roleName,
                    description,
                    isSystemRole);

                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created system role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogError(
                        "Failed to create role {RoleName}: {Errors}",
                        roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Role {RoleName} already exists", roleName);
            }
        }
    }
}