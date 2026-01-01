using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.DepartmentAggregate.Events;

namespace Apex.API.Core.Aggregates.DepartmentAggregate;

/// <summary>
/// Department aggregate root - represents an organizational unit within a tenant
/// Examples: Infrastructure, Security, Development, QA, DevOps
/// </summary>
public class Department : EntityBase, IAggregateRoot
{
    // Strong-typed ID
    private DepartmentId _id;
    public new DepartmentId Id
    {
        get => _id;
        private set => _id = value;
    }

    // Multi-tenant isolation
    private TenantId _tenantId;
    public TenantId TenantId
    {
        get => _tenantId;
        private set => _tenantId = value;
    }

    // Core Information
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Management
    public Guid? DepartmentManagerUserId { get; private set; }
    public bool IsActive { get; private set; }

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // EF Core constructor
    private Department() { }

    /// <summary>
    /// Creates a new department (factory method)
    /// </summary>
    public static Department Create(
        TenantId tenantId,
        string name,
        string description,
        Guid? departmentManagerUserId = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (name.Length < 2)
            throw new ArgumentException("Department name must be at least 2 characters", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Department name cannot exceed 100 characters", nameof(name));

        var department = new Department
        {
            Id = DepartmentId.CreateUnique(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            DepartmentManagerUserId = departmentManagerUserId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Raise domain event
        department.RegisterDomainEvent(new DepartmentCreatedEvent(
            department.Id,
            department.Name));

        return department;
    }

    /// <summary>
    /// Updates department details
    /// </summary>
    public void Update(string name, string description)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (name.Length < 2)
            throw new ArgumentException("Department name must be at least 2 characters", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Department name cannot exceed 100 characters", nameof(name));

        Name = name;
        Description = description;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns a department manager
    /// </summary>
    public void AssignManager(Guid managerUserId)
    {
        var previousManagerId = DepartmentManagerUserId;
        DepartmentManagerUserId = managerUserId;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new DepartmentManagerAssignedEvent(
            Id,
            managerUserId,
            previousManagerId));
    }

    /// <summary>
    /// Removes the department manager
    /// </summary>
    public void RemoveManager()
    {
        if (DepartmentManagerUserId == null)
            throw new InvalidOperationException("Department does not have a manager assigned");

        var previousManagerId = DepartmentManagerUserId.Value;
        DepartmentManagerUserId = null;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new DepartmentManagerRemovedEvent(Id, previousManagerId));
    }

    /// <summary>
    /// Deactivates the department
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Department is already inactive");

        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new DepartmentDeactivatedEvent(Id, Name));
    }

    /// <summary>
    /// Reactivates the department
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Department is already active");

        IsActive = true;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new DepartmentActivatedEvent(Id, Name));
    }

    public override string ToString()
    {
        return $"Department: {Name}";
    }
}
