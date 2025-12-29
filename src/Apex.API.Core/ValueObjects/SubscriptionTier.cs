using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Subscription tiers with embedded pricing and limits
/// </summary>
public sealed class SubscriptionTier : SmartEnum<SubscriptionTier>
{
    /// <summary>
    /// Free 14-day trial
    /// </summary>
    public static readonly SubscriptionTier Trial = new(
        nameof(Trial), 
        0, 
        monthlyPrice: 0m, 
        maxUsers: 5, 
        maxRequestsPerMonth: 50,
        maxStorageGB: 1);

    /// <summary>
    /// Starter tier - $99/month
    /// </summary>
    public static readonly SubscriptionTier Starter = new(
        nameof(Starter), 
        1, 
        monthlyPrice: 99m, 
        maxUsers: 10, 
        maxRequestsPerMonth: 100,
        maxStorageGB: 5);

    /// <summary>
    /// Professional tier - $299/month (Most Popular)
    /// </summary>
    public static readonly SubscriptionTier Professional = new(
        nameof(Professional), 
        2, 
        monthlyPrice: 299m, 
        maxUsers: 50, 
        maxRequestsPerMonth: int.MaxValue,
        maxStorageGB: 50);

    /// <summary>
    /// Enterprise tier - $799/month (Unlimited)
    /// </summary>
    public static readonly SubscriptionTier Enterprise = new(
        nameof(Enterprise), 
        3, 
        monthlyPrice: 799m, 
        maxUsers: int.MaxValue, 
        maxRequestsPerMonth: int.MaxValue,
        maxStorageGB: int.MaxValue);

    /// <summary>
    /// Monthly price in USD
    /// </summary>
    public decimal MonthlyPrice { get; }

    /// <summary>
    /// Maximum number of active users
    /// </summary>
    public int MaxUsers { get; }

    /// <summary>
    /// Maximum requests per month (int.MaxValue = unlimited)
    /// </summary>
    public int MaxRequestsPerMonth { get; }

    /// <summary>
    /// Maximum storage in GB (int.MaxValue = unlimited)
    /// </summary>
    public int MaxStorageGB { get; }

    /// <summary>
    /// Whether this tier has unlimited requests
    /// </summary>
    public bool HasUnlimitedRequests => MaxRequestsPerMonth == int.MaxValue;

    /// <summary>
    /// Whether this tier has unlimited users
    /// </summary>
    public bool HasUnlimitedUsers => MaxUsers == int.MaxValue;

    /// <summary>
    /// Whether this is the trial tier
    /// </summary>
    public bool IsTrial => this == Trial;

    private SubscriptionTier(
        string name, 
        int value, 
        decimal monthlyPrice, 
        int maxUsers, 
        int maxRequestsPerMonth,
        int maxStorageGB) 
        : base(name, value)
    {
        MonthlyPrice = monthlyPrice;
        MaxUsers = maxUsers;
        MaxRequestsPerMonth = maxRequestsPerMonth;
        MaxStorageGB = maxStorageGB;
    }

    /// <summary>
    /// Checks if upgrading to target tier
    /// </summary>
    public bool CanUpgradeTo(SubscriptionTier target)
    {
        return target.Value > this.Value;
    }

    /// <summary>
    /// Gets the Stripe price ID for this tier (for integration)
    /// </summary>
    public string GetStripePriceId(bool annual = false)
    {
        // These would be configured in appsettings, but showing the pattern
        return this switch
        {
            _ when this == Trial => string.Empty, // No Stripe price for trial
            _ when this == Starter => annual ? "price_starter_annual" : "price_starter_monthly",
            _ when this == Professional => annual ? "price_pro_annual" : "price_pro_monthly",
            _ when this == Enterprise => annual ? "price_enterprise_annual" : "price_enterprise_monthly",
            _ => throw new InvalidOperationException($"Unknown tier: {Name}")
        };
    }
}
