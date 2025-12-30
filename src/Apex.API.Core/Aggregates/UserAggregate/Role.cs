using Microsoft.AspNetCore.Identity;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.UserAggregate;

/// <summary>
/// Role entity with multi-tenant support
/// Extends IdentityRole for ASP.NET Core Identity integration
/// </summary>
public class Role : IdentityRole<Guid>
{
    // Multi-tenant isolation
    private TenantId _tenantId;
    public TenantId TenantId
    {
        get => _tenantId;
        private set => _tenantId = value;
    }

    public string Description { get; private set; } = string.Empty;
    public bool IsSystemRole { get; private set; } // System roles can't be deleted
    public DateTime CreatedDate { get; private set; }

    // EF Core constructor
    private Role() { }

    /// <summary>
    /// Creates a new role
    /// </summary>
    public static Role Create(
        TenantId tenantId,
        string name,
        string description,
        bool isSystemRole = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            IsSystemRole = isSystemRole,
            CreatedDate = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// Predefined system roles
    /// </summary>
    public static class SystemRoles
    {
        public const string TenantAdmin = "TenantAdmin";
        public const string User = "User";
        public const string Manager = "Manager";
        public const string ReadOnly = "ReadOnly";
    }
}
