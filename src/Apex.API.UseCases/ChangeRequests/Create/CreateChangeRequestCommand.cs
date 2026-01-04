using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Create;

/// <summary>
/// Command to create a new change request
/// </summary>
public record CreateChangeRequestCommand(
    string Title,
    string Description,
    string ChangeType,
    string Priority,
    string RiskLevel,
    string ImpactAssessment,
    string RollbackPlan,
    string AffectedSystems,
    DateTime? ScheduledStartDate = null,
    DateTime? ScheduledEndDate = null,
    string? ChangeWindow = null
) : IRequest<Result<ChangeRequestId>>;
