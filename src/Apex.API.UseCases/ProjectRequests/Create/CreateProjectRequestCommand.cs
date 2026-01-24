using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Create;

/// <summary>
/// Command to create a new ProjectRequest
/// </summary>
public record CreateProjectRequestCommand(
    string Title,
    string Description,
    string BusinessJustification,
    string? Priority = null,
    DateTime? DueDate = null,
    decimal? EstimatedBudget = null,
    DateTime? ProposedStartDate = null,
    DateTime? ProposedEndDate = null
) : IRequest<Result<ProjectRequestId>>;
