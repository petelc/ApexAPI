using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.GetById;

/// <summary>
/// Handler for getting a ProjectRequest by ID
/// </summary>
public class GetProjectRequestByIdHandler : IRequestHandler<GetProjectRequestByIdQuery, Result<ProjectRequestDto>>
{
    private readonly IReadRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProjectRequestByIdHandler> _logger;

    public GetProjectRequestByIdHandler(
        IReadRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ILogger<GetProjectRequestByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProjectRequestDto>> Handle(
        GetProjectRequestByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var projectRequest = await _repository.GetByIdAsync(query.ProjectRequestId, cancellationToken);

            if (projectRequest == null)
            {
                _logger.LogWarning(
                    "ProjectRequest not found: ProjectRequestId={ProjectRequestId}",
                    query.ProjectRequestId);

                return Result<ProjectRequestDto>.NotFound("ProjectRequest not found.");
            }

            // Verify tenant ownership (multi-tenant security)
            if (projectRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ProjectRequestId={ProjectRequestId}, TenantId={TenantId}",
                    query.ProjectRequestId,
                    _tenantContext.CurrentTenantId);

                return Result<ProjectRequestDto>.Forbidden();
            }

            var dto = new ProjectRequestDto(
                projectRequest.Id.Value,
                projectRequest.Title,
                projectRequest.Description,
                projectRequest.Status.Name,
                projectRequest.Priority.Name,
                projectRequest.CreatedByUserId,
                projectRequest.ReviewedByUserId,
                projectRequest.ApprovedByUserId,
                projectRequest.ConvertedByUserId,
                projectRequest.CreatedDate,
                projectRequest.SubmittedDate,
                projectRequest.ReviewStartedDate,
                projectRequest.ApprovedDate,
                projectRequest.DeniedDate,
                projectRequest.ConvertedDate,      // ✅ This exists
                projectRequest.DueDate,
                projectRequest.ReviewNotes,
                projectRequest.ApprovalNotes,
                projectRequest.DenialReason,
                projectRequest.ProjectId,
                projectRequest.IsOverdue(),
                projectRequest.GetDaysUntilDue());

            return Result<ProjectRequestDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving ProjectRequest: ProjectRequestId={ProjectRequestId}",
                query.ProjectRequestId);

            return Result<ProjectRequestDto>.Error("An error occurred while retrieving the ProjectRequest.");
        }
    }
}
