using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Subscription status states
/// </summary>
public sealed class SubscriptionStatus : SmartEnum<SubscriptionStatus>
{
    /// <summary>
    /// Active subscription with valid payment
    /// </summary>
    public static readonly SubscriptionStatus Active = new(nameof(Active), 0);

    /// <summary>
    /// Trial period (14 days)
    /// </summary>
    public static readonly SubscriptionStatus Trial = new(nameof(Trial), 1);

    /// <summary>
    /// Suspended due to non-payment or violation
    /// </summary>
    public static readonly SubscriptionStatus Suspended = new(nameof(Suspended), 2);

    /// <summary>
    /// Cancelled by user
    /// </summary>
    public static readonly SubscriptionStatus Cancelled = new(nameof(Cancelled), 3);

    /// <summary>
    /// Payment past due
    /// </summary>
    public static readonly SubscriptionStatus PastDue = new(nameof(PastDue), 4);

    private SubscriptionStatus(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Whether the subscription allows access to the system
    /// </summary>
    public bool AllowsAccess => this == Active || this == Trial;

    /// <summary>
    /// Whether the subscription requires payment
    /// </summary>
    public bool RequiresPayment => this == PastDue;

    /// <summary>
    /// Whether the subscription is terminated
    /// </summary>
    public bool IsTerminated => this == Cancelled || this == Suspended;
}
