using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.PutOnHold;

/// <summary>
/// Handler for putting a project on hold
/// </summary>
public class PutProjectOnHoldHandler : IRequestHandler<PutProjectOnHoldCommand, Result>
{
    private readonly IRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PutProjectOnHoldHandler> _logger;

    public PutProjectOnHoldHandler(
        IRepository<Project> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<PutProjectOnHoldHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        PutProjectOnHoldCommand command,
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

            // Put project on hold
            project.PutOnHold(_currentUserService.UserId, command.Reason);

            await _repository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation(
                "Project put on hold: ProjectId={ProjectId}, Reason={Reason}",
                project.Id,
                command.Reason);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot put project on hold: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error putting project on hold: ProjectId={ProjectId}",
                command.ProjectId);
            return Result.Error("An unexpected error occurred while putting project on hold.");
        }
    }
}
