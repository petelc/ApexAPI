using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.Complete;

public class CompleteProjectHandler : IRequestHandler<CompleteProjectCommand, Result>
{
    private readonly IRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompleteProjectHandler> _logger;

    public CompleteProjectHandler(
        IRepository<Project> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CompleteProjectHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(CompleteProjectCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project == null)
                return Result.NotFound("Project not found.");

            if (project.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            project.Complete(_currentUserService.UserId);

            await _repository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation("Project completed: ProjectId={ProjectId}", command.ProjectId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot complete project: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing project: ProjectId={ProjectId}", command.ProjectId);
            return Result.Error("An error occurred while completing the project.");
        }
    }
}