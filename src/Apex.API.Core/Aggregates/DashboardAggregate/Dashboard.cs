using Traxs.SharedKernel;

namespace Apex.API.Core.Aggregates.DashboardAggregate;

/// <summary>
/// Represents a dashboard entity within the system.
/// </summary>
public class Dashboard : EntityBase, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Dashboard(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
