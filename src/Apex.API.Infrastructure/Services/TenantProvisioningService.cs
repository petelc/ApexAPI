using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Infrastructure.Services;

/// <summary>
/// ULTRA-SIMPLIFIED: Uses raw SQL only, no DbContext or factory at all!
/// </summary>
public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        IConfiguration configuration,
        ILogger<TenantProvisioningService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProvisionTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        // Get tenant schema name from database first
        var schemaName = await GetTenantSchemaNameAsync(tenantId, cancellationToken);

        _logger.LogInformation("Starting provisioning for tenant {TenantId} with schema {SchemaName}", 
            tenantId, schemaName);

        try
        {
            await CreateSchemaAsync(schemaName, cancellationToken);
            _logger.LogInformation("Successfully provisioned tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task DeprovisionTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var schemaName = await GetTenantSchemaNameAsync(tenantId, cancellationToken);

        _logger.LogInformation("Starting deprovisioning for tenant {TenantId}", tenantId);

        try
        {
            await DeleteSchemaAsync(schemaName, cancellationToken);
            _logger.LogInformation("Successfully deprovisioned tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deprovision tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> SchemaExistsAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.schemas WHERE name = @SchemaName";
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count > 0;
    }

    private async Task<string> GetTenantSchemaNameAsync(TenantId tenantId, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT SchemaName FROM shared.Tenants WHERE Id = @TenantId";
        command.Parameters.AddWithValue("@TenantId", tenantId.Value);

        var schemaName = await command.ExecuteScalarAsync(cancellationToken) as string;

        if (string.IsNullOrEmpty(schemaName))
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found or has no schema name");
        }

        return schemaName;
    }

    private async Task CreateSchemaAsync(string schemaName, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var schemaExists = await SchemaExistsAsync(schemaName, cancellationToken);

        if (!schemaExists)
        {
            var createSchemaCommand = connection.CreateCommand();
            createSchemaCommand.CommandText = $"CREATE SCHEMA [{schemaName}]";
            await createSchemaCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Created schema {SchemaName}", schemaName);
        }
        else
        {
            _logger.LogWarning("Schema {SchemaName} already exists", schemaName);
        }
    }

    private async Task DeleteSchemaAsync(string schemaName, CancellationToken cancellationToken)
    {
        var protectedSchemas = new[] { "shared", "dbo", "sys", "guest", "INFORMATION_SCHEMA" };
        if (protectedSchemas.Contains(schemaName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot delete protected schema: {schemaName}");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var dropObjectsCommand = connection.CreateCommand();
        dropObjectsCommand.CommandText = $@"
            DECLARE @sql NVARCHAR(MAX) = '';
            SELECT @sql = @sql + 'DROP TABLE IF EXISTS [{schemaName}].' + QUOTENAME(name) + ';'
            FROM sys.tables
            WHERE schema_id = SCHEMA_ID(@SchemaName);
            EXEC sp_executesql @sql;
            DROP SCHEMA IF EXISTS [{schemaName}];";
        dropObjectsCommand.Parameters.AddWithValue("@SchemaName", schemaName);

        await dropObjectsCommand.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Deleted schema {SchemaName}", schemaName);
    }
}
