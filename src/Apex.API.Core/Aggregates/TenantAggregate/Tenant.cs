using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate.Events;
using System.Text.RegularExpressions;

namespace Apex.API.Core.Aggregates.TenantAggregate;

/// <summary>
/// Tenant aggregate root representing a company/organization using APEX
/// </summary>
public class Tenant : EntityBase, IAggregateRoot
{
    // Identity
    private TenantId _id;
    public new TenantId Id
    {
        get => _id;
        private set => _id = value;
    }

    // Company Information
    public string CompanyName { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;

    /// <summary>
    /// Database schema name for this tenant 
    /// </summary>
    public string SchemaName { get; private set; } = string.Empty;  // ✅ Has setter

    // Subscription
    public SubscriptionTier Tier { get; private set; } = SubscriptionTier.Trial;
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Trial;

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? TrialEndsDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // Status
    public bool IsActive { get; private set; }

    // Usage Limits (set by tier)
    public int MaxUsers { get; private set; }
    public int MaxRequestsPerMonth { get; private set; }
    public int MaxStorageGB { get; private set; }

    // Data Region (for multi-region SaaS)
    public string Region { get; private set; } = "USEast";

    // EF Core constructor
    private Tenant() { }

    /// <summary>
    /// Creates a new tenant (factory method)
    /// </summary>
    public static Tenant Create(string companyName, string subdomain, string region = "USEast")
    {
        Guard.Against.NullOrWhiteSpace(companyName, nameof(companyName));
        Guard.Against.NullOrWhiteSpace(subdomain, nameof(subdomain));

        // Validate subdomain format
        if (!IsValidSubdomain(subdomain))
        {
            throw new ArgumentException(
                "Subdomain must be 3-63 characters, alphanumeric and hyphens only, " +
                "cannot start or end with hyphen",
                nameof(subdomain));
        }

        var tenant = new Tenant
        {
            Id = TenantId.CreateUnique(),
            CompanyName = companyName,
            Subdomain = subdomain.ToLowerInvariant(),
            SchemaName = $"tenant_{subdomain.ToLowerInvariant()}",
            Tier = SubscriptionTier.Trial,
            Status = SubscriptionStatus.Trial,
            CreatedDate = DateTime.UtcNow,
            TrialEndsDate = DateTime.UtcNow.AddDays(14),
            IsActive = true,
            Region = region
        };

        // Set limits based on trial tier
        tenant.UpdateLimitsForTier(SubscriptionTier.Trial);

        // Raise domain event
        tenant.RegisterDomainEvent(new TenantCreatedEvent(
            tenant.Id.Value,
            companyName,
            subdomain,
            tenant.SchemaName));  // ✅ Now this will have a value!

        return tenant;
    }

    /// <summary>
    /// Upgrades tenant to a higher subscription tier
    /// </summary>
    public void UpgradeTier(SubscriptionTier newTier)
    {
        Guard.Against.Null(newTier, nameof(newTier));

        if (!Tier.CanUpgradeTo(newTier))
        {
            throw new InvalidOperationException(
                $"Cannot upgrade from {Tier.Name} to {newTier.Name}. Can only upgrade to higher tier.");
        }

        var oldTier = Tier;
        Tier = newTier;
        Status = SubscriptionStatus.Active;
        TrialEndsDate = null; // No longer in trial
        LastModifiedDate = DateTime.UtcNow;

        // Update limits
        UpdateLimitsForTier(newTier);

        // Raise domain event
        RegisterDomainEvent(new TenantUpgradedEvent(Id, oldTier, newTier));
    }

    /// <summary>
    /// Downgrades tenant to a lower tier (used when subscription cancelled)
    /// </summary>
    public void DowngradeTier(SubscriptionTier newTier)
    {
        Guard.Against.Null(newTier, nameof(newTier));

        Tier = newTier;
        LastModifiedDate = DateTime.UtcNow;
        UpdateLimitsForTier(newTier);

        // Note: No event raised for downgrade as it's typically part of cancellation
    }

    /// <summary>
    /// Suspends the tenant (for non-payment, violation, etc.)
    /// </summary>
    public void Suspend(string reason)
    {
        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        if (!IsActive) return; // Already suspended

        Status = SubscriptionStatus.Suspended;
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TenantSuspendedEvent(Id, reason));
    }

    /// <summary>
    /// Reactivates a suspended tenant
    /// </summary>
    public void Reactivate()
    {
        if (IsActive) return; // Already active

        Status = SubscriptionStatus.Active;
        IsActive = true;
        LastModifiedDate = DateTime.UtcNow;

        // Could raise TenantReactivatedEvent if needed
    }

    /// <summary>
    /// Cancels the subscription
    /// </summary>
    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;

        // Could raise TenantCancelledEvent if needed
    }

    /// <summary>
    /// Marks trial as expired
    /// </summary>
    public void ExpireTrial()
    {
        if (Tier != SubscriptionTier.Trial)
            throw new InvalidOperationException("Tenant is not in trial");

        Status = SubscriptionStatus.Suspended;
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;

        // Could raise TrialExpiredEvent if needed
    }

    /// <summary>
    /// Checks if tenant's trial has expired
    /// </summary>
    public bool IsTrialExpired()
    {
        return Tier == SubscriptionTier.Trial &&
               TrialEndsDate.HasValue &&
               DateTime.UtcNow > TrialEndsDate.Value;
    }

    /// <summary>
    /// Gets days remaining in trial
    /// </summary>
    public int? GetTrialDaysRemaining()
    {
        if (!TrialEndsDate.HasValue) return null;

        var daysRemaining = (TrialEndsDate.Value - DateTime.UtcNow).Days;
        return daysRemaining > 0 ? daysRemaining : 0;
    }

    /// <summary>
    /// Checks if usage is allowed (tenant is active and not suspended)
    /// </summary>
    public bool CanUseSystem()
    {
        return IsActive && Status.AllowsAccess;
    }

    /// <summary>
    /// Updates usage limits based on subscription tier
    /// </summary>
    private void UpdateLimitsForTier(SubscriptionTier tier)
    {
        MaxUsers = tier.MaxUsers;
        MaxRequestsPerMonth = tier.MaxRequestsPerMonth;
        MaxStorageGB = tier.MaxStorageGB;
    }

    /// <summary>
    /// Validates subdomain format (3-63 chars, alphanumeric + hyphens, no leading/trailing hyphen)
    /// </summary>
    private static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain)) return false;
        if (subdomain.Length < 3 || subdomain.Length > 63) return false;

        // Must match: alphanumeric, hyphens allowed, but not at start/end
        var regex = new Regex(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.IgnoreCase);
        return regex.IsMatch(subdomain);
    }

    /// <summary>
    /// Override ToString for debugging
    /// </summary>
    public override string ToString()
    {
        return $"{CompanyName} ({Subdomain}) - {Tier.Name} - {Status.Name}";
    }
}
