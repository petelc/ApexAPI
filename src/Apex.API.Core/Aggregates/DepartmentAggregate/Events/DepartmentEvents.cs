using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.DepartmentAggregate.Events;

/// <summary>
/// Event raised when a new department is created
/// </summary>
public class DepartmentCreatedEvent : DomainEventBase
{
    public DepartmentId DepartmentId { get; }
    public string DepartmentName { get; }

    public DepartmentCreatedEvent(DepartmentId departmentId, string departmentName)
    {
        DepartmentId = departmentId;
        DepartmentName = departmentName;
    }
}

/// <summary>
/// Event raised when a department manager is assigned
/// </summary>
public class DepartmentManagerAssignedEvent : DomainEventBase
{
    public DepartmentId DepartmentId { get; }
    public Guid ManagerUserId { get; }
    public Guid? PreviousManagerUserId { get; }

    public DepartmentManagerAssignedEvent(
        DepartmentId departmentId, 
        Guid managerUserId, 
        Guid? previousManagerUserId)
    {
        DepartmentId = departmentId;
        ManagerUserId = managerUserId;
        PreviousManagerUserId = previousManagerUserId;
    }
}

/// <summary>
/// Event raised when a department manager is removed
/// </summary>
public class DepartmentManagerRemovedEvent : DomainEventBase
{
    public DepartmentId DepartmentId { get; }
    public Guid PreviousManagerUserId { get; }

    public DepartmentManagerRemovedEvent(DepartmentId departmentId, Guid previousManagerUserId)
    {
        DepartmentId = departmentId;
        PreviousManagerUserId = previousManagerUserId;
    }
}

/// <summary>
/// Event raised when a department is deactivated
/// </summary>
public class DepartmentDeactivatedEvent : DomainEventBase
{
    public DepartmentId DepartmentId { get; }
    public string DepartmentName { get; }

    public DepartmentDeactivatedEvent(DepartmentId departmentId, string departmentName)
    {
        DepartmentId = departmentId;
        DepartmentName = departmentName;
    }
}

/// <summary>
/// Event raised when a department is activated
/// </summary>
public class DepartmentActivatedEvent : DomainEventBase
{
    public DepartmentId DepartmentId { get; }
    public string DepartmentName { get; }

    public DepartmentActivatedEvent(DepartmentId departmentId, string departmentName)
    {
        DepartmentId = departmentId;
        DepartmentName = departmentName;
    }
}
