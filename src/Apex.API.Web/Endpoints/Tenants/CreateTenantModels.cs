namespace Apex.API.Web.Endpoints.Tenants;

/// <summary>
/// Request model for tenant signup
/// </summary>
public record CreateTenantRequest
{
    /// <summary>
    /// Company name
    /// </summary>
    /// <example>Acme Corporation</example>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Unique subdomain for the company (e.g., "acmecorp" for acmecorp.apex.cloud)
    /// </summary>
    /// <example>acmecorp</example>
    public string Subdomain { get; init; } = string.Empty;

    /// <summary>
    /// Admin user's email address
    /// </summary>
    /// <example>admin@acmecorp.com</example>
    public string AdminEmail { get; init; } = string.Empty;

    /// <summary>
    /// Admin user's first name
    /// </summary>
    /// <example>John</example>
    public string AdminFirstName { get; init; } = string.Empty;

    /// <summary>
    /// Admin user's last name
    /// </summary>
    /// <example>Doe</example>
    public string AdminLastName { get; init; } = string.Empty;

    /// <summary>
    /// Data region (optional, defaults to USEast)
    /// </summary>
    /// <example>USEast</example>
    public string Region { get; init; } = "USEast";
}

/// <summary>
/// Response model for successful tenant signup
/// </summary>
public record CreateTenantResponse
{
    /// <summary>
    /// Tenant ID (GUID)
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Subdomain assigned
    /// </summary>
    public string Subdomain { get; init; } = string.Empty;

    /// <summary>
    /// Full tenant URL
    /// </summary>
    public string TenantUrl { get; init; } = string.Empty;

    /// <summary>
    /// Subscription tier
    /// </summary>
    public string SubscriptionTier { get; init; } = string.Empty;

    /// <summary>
    /// Trial expiration date (if applicable)
    /// </summary>
    public DateTime? TrialEndsDate { get; init; }

    /// <summary>
    /// Days remaining in trial
    /// </summary>
    public int? TrialDaysRemaining { get; init; }

    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
