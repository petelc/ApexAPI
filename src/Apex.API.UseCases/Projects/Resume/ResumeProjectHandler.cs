using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.Resume;

/// <summary>
/// Handler for resuming a project from on-hold status
/// </summary>
public class ResumeProjectHandler : IRequestHandler<ResumeProjectCommand, Result>
{
    private readonly IRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ResumeProjectHandler> _logger;

    public ResumeProjectHandler(
        IRepository<Project> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ResumeProjectHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        ResumeProjectCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project == null)
            {
                _logger.LogWarning("Project not found: ProjectId={ProjectId}", command.ProjectId);
                return Result.NotFound("Project not found.");
            }

            // Verify tenant ownership
            if (project.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ProjectId={ProjectId}, TenantId={TenantId}",
                    command.ProjectId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            // Resume project
            project.Resume(_currentUserService.UserId);

            await _repository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation(
                "Project resumed: ProjectId={ProjectId}",
                project.Id);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot resume project: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error resuming project: ProjectId={ProjectId}",
                command.ProjectId);
            return Result.Error("An unexpected error occurred while resuming project.");
        }
    }
}
