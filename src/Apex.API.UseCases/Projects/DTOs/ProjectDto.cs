
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.UseCases.Users.DTOs;
using Apex.API.UseCases.Users.Interfaces;

namespace Apex.API.UseCases.Projects.DTOs;

public sealed record ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    // Add user fields
    public Guid CreatedByUserId { get; init; }
    public UserSummaryDto? CreatedByUser { get; init; }

    public Guid? ProjectManagerUserId { get; init; }
    public UserSummaryDto? ProjectManager { get; init; }
}

// Mapping extension




