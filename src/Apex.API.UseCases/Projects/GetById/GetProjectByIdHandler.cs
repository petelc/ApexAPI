using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Projects.DTOs;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.GetById;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IReadRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProjectByIdHandler> _logger;

    public GetProjectByIdHandler(
        IReadRepository<Project> repository,
        ITenantContext tenantContext,
        ILogger<GetProjectByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProjectDto>> Handle(
        GetProjectByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _repository.GetByIdAsync(query.ProjectId, cancellationToken);

            if (project == null)
            {
                _logger.LogWarning("Project not found: ProjectId={ProjectId}", query.ProjectId);
                return Result<ProjectDto>.NotFound("Project not found.");
            }

            // Verify tenant ownership
            if (project.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ProjectId={ProjectId}, TenantId={TenantId}",
                    query.ProjectId,
                    _tenantContext.CurrentTenantId);
                return Result<ProjectDto>.Forbidden();
            }

            var dto = new ProjectDto(
                project.Id.Value,
                project.Name,
                project.Description,
                project.Status.Name,
                project.Priority.Name,
                project.ProjectRequestId,
                project.Budget,
                project.StartDate,
                project.EndDate,
                project.ActualStartDate,
                project.ActualEndDate,
                project.CreatedByUserId,
                project.ProjectManagerUserId,
                project.CreatedDate,
                project.LastModifiedDate,
                project.IsOverdue(),
                project.GetDaysUntilDeadline(),
                project.GetDurationDays());

            return Result<ProjectDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project: ProjectId={ProjectId}", query.ProjectId);
            return Result<ProjectDto>.Error("An error occurred while retrieving the project.");
        }
    }
}