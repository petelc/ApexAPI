using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Update;

/// <summary>
/// Command to update a ProjectRequest (only allowed in Draft status)
/// </summary>
public record UpdateProjectRequestCommand(
    ProjectRequestId ProjectRequestId,
    string Title,
    string Description,
    string BusinessJustification,
    string? Priority = null,
    DateTime? DueDate = null,
    decimal? EstimatedBudget = null,
    DateTime? ProposedStartDate = null,
    DateTime? ProposedEndDate = null
) : IRequest<Result>;