using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Aggregates.ProjectAggregate;

namespace Apex.API.Infrastructure.Data;

/// <summary>
/// Main database context with multi-tenant support and Identity
/// </summary>
public class ApexDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public ApexDbContext(
        DbContextOptions<ApexDbContext> options,
        IDomainEventDispatcher? dispatcher = null)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    // Aggregates
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ProjectRequest> ProjectRequests => Set<ProjectRequest>(); // ✅ RENAMED
    public DbSet<Project> Projects => Set<Project>();  // ✅ NEW!

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important: Call base for Identity

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApexDbContext).Assembly);

        // Configure Identity tables to use shared schema
        modelBuilder.Entity<User>().ToTable("Users", "shared");
        modelBuilder.Entity<Role>().ToTable("Roles", "shared");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "shared");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "shared");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "shared");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "shared");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "shared");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        if (_dispatcher != null)
        {
            var entitiesWithEvents = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
