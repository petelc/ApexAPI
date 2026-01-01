using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.ConvertFromProjectRequest;

/// <summary>
/// Handler for converting an approved ProjectRequest into a Project
/// </summary>
public class ConvertProjectRequestToProjectHandler : IRequestHandler<ConvertProjectRequestToProjectCommand, Result<ProjectId>>
{
    private readonly IRepository<ProjectRequest> _projectRequestRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ConvertProjectRequestToProjectHandler> _logger;

    public ConvertProjectRequestToProjectHandler(
        IRepository<ProjectRequest> projectRequestRepository,
        IRepository<Project> projectRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ConvertProjectRequestToProjectHandler> logger)
    {
        _projectRequestRepository = projectRequestRepository;
        _projectRepository = projectRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ProjectId>> Handle(
        ConvertProjectRequestToProjectCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Converting ProjectRequest to Project: ProjectRequestId={ProjectRequestId}, User={UserId}",
                command.ProjectRequestId,
                _currentUserService.UserId);

            // 1. Get the ProjectRequest
            var projectRequest = await _projectRequestRepository.GetByIdAsync(
                command.ProjectRequestId,
                cancellationToken);

            if (projectRequest == null)
            {
                _logger.LogWarning("ProjectRequest not found: {ProjectRequestId}", command.ProjectRequestId);
                return Result<ProjectId>.NotFound("Project request not found.");
            }

            // 2. Verify tenant ownership
            if (projectRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized conversion attempt: ProjectRequestId={ProjectRequestId}, TenantId={TenantId}",
                    command.ProjectRequestId,
                    _tenantContext.CurrentTenantId);
                return Result<ProjectId>.Forbidden();
            }

            // 3. Verify ProjectRequest is approved
            if (!projectRequest.Status.CanConvertToProject)
            {
                _logger.LogWarning(
                    "Cannot convert ProjectRequest in {Status} status",
                    projectRequest.Status.Name);
                return Result<ProjectId>.Error(
                    $"Project request must be in Approved status to convert. Current status: {projectRequest.Status.Name}");
            }

            // 4. Create Project from ProjectRequest
            var project = Project.CreateFromProjectRequest(
                projectRequest.TenantId,
                projectRequest.Id.Value,
                projectRequest.Title,
                projectRequest.Description,
                projectRequest.Priority,
                _currentUserService.UserId,
                command.StartDate,
                command.EndDate,
                command.Budget);

            // 5. Assign Project Manager if specified
            if (command.ProjectManagerUserId.HasValue)
            {
                project.AssignProjectManager(
                    command.ProjectManagerUserId.Value,
                    _currentUserService.UserId);
            }

            // 6. Save Project (this will trigger ProjectCreatedEvent)
            await _projectRepository.AddAsync(project, cancellationToken);

            // 7. Mark ProjectRequest as Converted
            projectRequest.MarkAsConverted(project.Id.Value, _currentUserService.UserId);
            await _projectRequestRepository.UpdateAsync(projectRequest, cancellationToken);

            _logger.LogInformation(
                "✅ Successfully converted ProjectRequest to Project: ProjectRequestId={ProjectRequestId}, ProjectId={ProjectId}",
                command.ProjectRequestId,
                project.Id);

            return Result<ProjectId>.Success(project.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot convert project request: {Message}", ex.Message);
            return Result<ProjectId>.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error converting project request: {Message}", ex.Message);
            return Result<ProjectId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error converting ProjectRequest to Project: ProjectRequestId={ProjectRequestId}",
                command.ProjectRequestId);
            return Result<ProjectId>.Error("An unexpected error occurred while converting the project request.");
        }
    }
}