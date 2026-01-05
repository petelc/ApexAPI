using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.GetById;

/// <summary>
/// Handler for getting a ChangeRequest by ID
/// </summary>
public class GetChangeRequestByIdHandler : IRequestHandler<GetChangeRequestByIdQuery, Result<ChangeRequestDto>>
{
    private readonly IReadRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetChangeRequestByIdHandler> _logger;
    public GetChangeRequestByIdHandler(
        IReadRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ILogger<GetChangeRequestByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ChangeRequestDto>> Handle(
        GetChangeRequestByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var changeRequest = await _repository.GetByIdAsync(query.ChangeRequestId, cancellationToken);

            if (changeRequest == null)
            {
                _logger.LogWarning(
                    "ChangeRequest not found: ChangeRequestId={ChangeRequestId}",
                    query.ChangeRequestId);

                return Result<ChangeRequestDto>.NotFound("ChangeRequest not found.");
            }

            // Verify tenant ownership (multi-tenant security)
            if (changeRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ChangeRequestId={ChangeRequestId}, TenantId={TenantId}",
                    query.ChangeRequestId,
                    _tenantContext.CurrentTenantId);

                return Result<ChangeRequestDto>.Forbidden();
            }

            var dto = new ChangeRequestDto(
                changeRequest.Id.Value,
                changeRequest.Title,
                changeRequest.Description,
                changeRequest.Status.Name,
                changeRequest.ChangeType.Name,
                changeRequest.Priority.Name,
                changeRequest.RiskLevel.Name,
                changeRequest.ImpactAssessment,
                changeRequest.RollbackPlan,
                changeRequest.AffectedSystems,
                changeRequest.ScheduledStartDate,
                changeRequest.ScheduledEndDate,
                changeRequest.ChangeWindow,
                changeRequest.RequiresCABApproval,
                changeRequest.CreatedByUserId,
                changeRequest.ReviewedByUserId,
                changeRequest.ApprovedByUserId,
                changeRequest.ReviewNotes,
                changeRequest.ApprovalNotes,
                changeRequest.DenialReason,
                changeRequest.CreatedDate,
                changeRequest.SubmittedDate,
                changeRequest.ReviewStartedDate,
                changeRequest.ApprovedDate,
                changeRequest.DeniedDate,
                changeRequest.CompletedDate,
                changeRequest.FailedDate,
                changeRequest.RolledBackDate,
                changeRequest.ProjectId,
                changeRequest.IsOverdue());

            return Result<ChangeRequestDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving ProjectRequest: ProjectRequestId={ProjectRequestId}",
                query.ChangeRequestId);

            return Result<ChangeRequestDto>.Error("An error occurred while retrieving the ChangeRequest.");
        }
    }
}
