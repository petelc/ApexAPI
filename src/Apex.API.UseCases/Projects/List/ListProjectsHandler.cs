using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Projects.GetById;

namespace Apex.API.UseCases.Projects.List;

public class ListProjectsHandler : IRequestHandler<ListProjectsQuery, Result<PagedProjectList>>
{
    private readonly IReadRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListProjectsHandler> _logger;

    public ListProjectsHandler(
        IReadRepository<Project> repository,
        ITenantContext tenantContext,
        ILogger<ListProjectsHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PagedProjectList>> Handle(
        ListProjectsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var allProjects = await _repository.ListAsync(cancellationToken);

            var projects = allProjects
                .Where(p => p.TenantId == _tenantContext.CurrentTenantId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (ProjectStatus.TryFromName(query.Status, out var status))
                {
                    projects = projects.Where(p => p.Status == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                if (RequestPriority.TryFromName(query.Priority, out var priority))
                {
                    projects = projects.Where(p => p.Priority == priority);
                }
            }

            if (query.ProjectManagerUserId.HasValue)
            {
                projects = projects.Where(p => p.ProjectManagerUserId == query.ProjectManagerUserId.Value);
            }

            if (query.IsOverdue.HasValue && query.IsOverdue.Value)
            {
                projects = projects.Where(p => p.IsOverdue());
            }

            var totalCount = projects.Count();

            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedProjects = projects
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(query.PageSize)
                .ToList();

            var dtos = pagedProjects.Select(p => new ProjectDto(
                p.Id.Value,
                p.Name,
                p.Description,
                p.Status.Name,
                p.Priority.Name,
                p.ProjectRequestId,
                p.Budget,
                p.StartDate,
                p.EndDate,
                p.ActualStartDate,
                p.ActualEndDate,
                p.CreatedByUserId,
                p.ProjectManagerUserId,
                p.CreatedDate,
                p.LastModifiedDate,
                p.IsOverdue(),
                p.GetDaysUntilDeadline(),
                p.GetDurationDays()
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var result = new PagedProjectList(dtos, totalCount, query.PageNumber, query.PageSize, totalPages);

            _logger.LogInformation(
                "Listed projects: Count={Count}, Page={PageNumber}, TenantId={TenantId}",
                totalCount,
                query.PageNumber,
                _tenantContext.CurrentTenantId);

            return Result<PagedProjectList>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing projects");
            return Result<PagedProjectList>.Error("An error occurred while listing projects.");
        }
    }
}