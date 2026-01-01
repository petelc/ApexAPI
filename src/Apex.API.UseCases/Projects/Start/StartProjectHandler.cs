using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Projects.Start;

public class StartProjectHandler : IRequestHandler<StartProjectCommand, Result>
{
    private readonly IRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<StartProjectHandler> _logger;

    public StartProjectHandler(
        IRepository<Project> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<StartProjectHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(StartProjectCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project == null)
                return Result.NotFound("Project not found.");

            if (project.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            project.Start(_currentUserService.UserId);

            await _repository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation("Project started: ProjectId={ProjectId}", command.ProjectId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot start project: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting project: ProjectId={ProjectId}", command.ProjectId);
            return Result.Error("An error occurred while starting the project.");
        }
    }
}