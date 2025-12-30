using Microsoft.AspNetCore.Identity;
using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.UserAggregate;

/// <summary>
/// User entity with multi-tenant support
/// Extends IdentityUser for ASP.NET Core Identity integration
/// </summary>
public class User : IdentityUser<Guid>, IAggregateRoot
{
    // Multi-tenant isolation
    private TenantId _tenantId;
    public TenantId TenantId
    {
        get => _tenantId;
        private set => _tenantId = value;
    }

    // Personal Information
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}";

    // Status
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // User preferences
    public string? TimeZone { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    // Domain events
    private readonly List<DomainEventBase> _domainEvents = new();
    public IReadOnlyCollection<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    // EF Core constructor
    private User() { }

    /// <summary>
    /// Creates a new user (factory method)
    /// </summary>
    public static User Create(
        TenantId tenantId,
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? timeZone = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            UserName = email, // Use email as username
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            TimeZone = timeZone,
            EmailConfirmed = false, // Require email confirmation
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true, // Enable account lockout for security
            AccessFailedCount = 0,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        // TODO: Raise UserCreatedEvent

        return user;
    }

    /// <summary>
    /// Updates user profile information
    /// </summary>
    public void UpdateProfile(
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? timeZone = null)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        TimeZone = timeZone;
        LastModifiedDate = DateTime.UtcNow;

        // TODO: Raise UserProfileUpdatedEvent
    }

    /// <summary>
    /// Records successful login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        AccessFailedCount = 0; // Reset failed attempts

        // TODO: Raise UserLoggedInEvent
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;

        // TODO: Raise UserDeactivatedEvent
    }

    /// <summary>
    /// Reactivates the user account
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        LockoutEnd = null; // Clear any lockout
        LastModifiedDate = DateTime.UtcNow;

        // TODO: Raise UserReactivatedEvent
    }

    /// <summary>
    /// Updates profile image
    /// </summary>
    public void UpdateProfileImage(string imageUrl)
    {
        ProfileImageUrl = imageUrl;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void RegisterDomainEvent(DomainEventBase domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
