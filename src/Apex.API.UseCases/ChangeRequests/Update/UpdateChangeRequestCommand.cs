using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Update;

/// <summary>
/// Command to update a ChangeRequest (only allowed in Draft status)
/// </summary>
public record UpdateChangeRequestCommand(
    ChangeRequestId ChangeRequestId,
    string Title,
    string Description,
    string ImpactAssessment,
    string RollbackPlan,
    string AffectedSystems,
    string? Priority = null,
    string? RiskLevel = null,
    DateTime? ScheduledStartDate = null,
    DateTime? ScheduledEndDate = null,
    string? ChangeWindow = null

) : IRequest<Result>;