using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.UseCases.Projects.DTOs;
using Apex.API.UseCases.Projects.Specifications;
using Ardalis.Result;
using Traxs.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Apex.API.UseCases.Projects.List;

/// <summary>
/// Handler for listing projects
/// MATCHES your actual ListProjectsQuery parameters
/// Works with positional ProjectDto record
/// </summary>
public class ListProjectsHandler : IRequestHandler<ListProjectsQuery, Result<ListProjectsResponse>>
{
    private readonly IReadRepository<Project> _repository;
    private readonly ILogger<ListProjectsHandler> _logger;

    public ListProjectsHandler(
        IReadRepository<Project> repository,
        ILogger<ListProjectsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ListProjectsResponse>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create specification for filtering and pagination
            var listSpec = new ListProjectsSpec(
                request.Status,
                request.Priority,
                request.ProjectManagerUserId,
                request.CreatedByUserId,
                request.StartDate,
                request.EndDate,
                request.PageNumber,
                request.PageSize);

            // Create specification for counting (same filters, no pagination)
            var countSpec = new CountProjectsSpec(
                request.Status,
                request.Priority,
                request.ProjectManagerUserId,
                request.CreatedByUserId,
                request.StartDate,
                request.EndDate);

            // Execute queries using EfRepository
            var projects = await _repository.ListAsync(listSpec, cancellationToken);
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // Map domain entities to DTOs using positional record constructor
            var dtos = projects.Select(p => new ProjectDto(
                // Positional parameters in order:
                Id: p.Id.Value,
                Name: p.Name,
                Description: p.Description,
                Status: p.Status.Name,
                Priority: p.Priority.Name,
                ProjectRequestId: p.ProjectRequestId,
                Budget: p.Budget,
                StartDate: p.StartDate,
                EndDate: p.EndDate,
                ActualStartDate: p.ActualStartDate,
                ActualEndDate: p.ActualEndDate,
                CreatedByUserId: p.CreatedByUserId,
                ProjectManagerUserId: p.ProjectManagerUserId,
                CreatedDate: p.CreatedDate,
                LastModifiedDate: p.LastModifiedDate,
                IsOverdue: p.IsOverdue(),
                DaysUntilDeadline: p.GetDaysUntilDeadline(),
                DurationDays: p.GetDurationDays(),
                CreatedByUser: null,  // Populated in Web layer
                ProjectManagerUser: null  // Populated in Web layer
            )).ToList();

            var response = new ListProjectsResponse(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing projects");
            return Result.Error("An error occurred while retrieving projects");
        }
    }
}
