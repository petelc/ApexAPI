using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.DepartmentAggregate;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Data.SeedData;

public class DepartmentSeeder
{
    private readonly ApexDbContext _context;
    private readonly ILogger<DepartmentSeeder> _logger;

    public DepartmentSeeder(ApexDbContext context, ILogger<DepartmentSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDepartmentsForTenantAsync(TenantId tenantId)
    {
        // Check if departments already exist for this tenant
        var existingCount = await _context.Departments
            .CountAsync(d => d.TenantId == tenantId);

        if (existingCount > 0)
        {
            _logger.LogInformation("Departments already seeded for tenant {TenantId}", tenantId);
            return;
        }

        _logger.LogInformation("Seeding departments for tenant {TenantId}...", tenantId);

        var departments = new[]
        {
            Department.Create(
                tenantId,
                "Application Development",
                "Designs, develops, and maintains software applications and systems"),

            Department.Create(
                tenantId,
                "Infrastructure & Operations",
                "Manages servers, networks, and IT infrastructure"),

            Department.Create(
                tenantId,
                "Information Security",
                "Protects systems, networks, and data from cyber threats"),

            Department.Create(
                tenantId,
                "Quality Assurance",
                "Tests and validates software quality and performance"),

            Department.Create(
                tenantId,
                "DevOps & Automation",
                "Automates deployment pipelines and manages CI/CD processes"),

            Department.Create(
                tenantId,
                "Database Administration",
                "Manages and maintains database systems and data integrity"),

            Department.Create(
                tenantId,
                "Network Engineering",
                "Designs and maintains network infrastructure and connectivity"),

            Department.Create(
                tenantId,
                "Help Desk & Support",
                "Provides technical support and troubleshooting to end users")
        };

        await _context.Departments.AddRangeAsync(departments);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Seeded {Count} departments for tenant {TenantId}",
            departments.Length, tenantId);
    }
}