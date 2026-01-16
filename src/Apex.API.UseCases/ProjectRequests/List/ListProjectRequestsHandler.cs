using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.UseCases.ProjectRequests.DTOs;
using Apex.API.UseCases.ProjectRequests.Specifications;
using Ardalis.Result;
using Traxs.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Apex.API.UseCases.ProjectRequests.List;

/// <summary>
/// Query for listing project requests with filtering and pagination
/// </summary>
public record ListProjectRequestsQuery(
    string? Status,
    string? Priority,
    Guid? AssignedToUserId,
    Guid? CreatedByUserId,
    bool? IsOverdue,
    int PageNumber,
    int PageSize
) : IRequest<Result<ListProjectRequestsResponse>>;

/// <summary>
/// Response with paginated project requests (WITHOUT user information - added in Web layer)
/// </summary>
public record ListProjectRequestsResponse(
    List<ProjectRequestDto> ProjectRequests,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Handler for listing project requests
/// Uses EfRepository from Traxs.SharedKernel with Ardalis.Specification
/// NO Infrastructure dependency - uses repository abstraction!
/// </summary>
public class ListProjectRequestsHandler : IRequestHandler<ListProjectRequestsQuery, Result<ListProjectRequestsResponse>>
{
    private readonly IReadRepository<ProjectRequest> _repository;
    private readonly ILogger<ListProjectRequestsHandler> _logger;

    public ListProjectRequestsHandler(
        IReadRepository<ProjectRequest> repository,
        ILogger<ListProjectRequestsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ListProjectRequestsResponse>> Handle(
        ListProjectRequestsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create specification for filtering and pagination
            var listSpec = new ListProjectRequestsSpec(
                request.Status,
                request.Priority,
                request.AssignedToUserId,
                request.CreatedByUserId,
                request.IsOverdue,
                request.PageNumber,
                request.PageSize);

            // Create specification for counting (same filters, no pagination)
            var countSpec = new CountProjectRequestsSpec(
                request.Status,
                request.Priority,
                request.AssignedToUserId,
                request.CreatedByUserId,
                request.IsOverdue);

            // Execute queries using EfRepository
            var projectRequests = await _repository.ListAsync(listSpec, cancellationToken);
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // Map domain entities to DTOs (NO user information yet - that's in Web layer)
            var dtos = projectRequests.Select(pr => new ProjectRequestDto
            {
                Id = pr.Id.Value,
                Title = pr.Title,
                Description = pr.Description,
                BusinessJustification = string.Empty,
                Status = pr.Status.Name,
                Priority = pr.Priority.Name,

                // User IDs only - no user objects yet
                RequestingUserId = pr.CreatedByUserId,
                RequestingUser = null, // Populated in Web layer

                AssignedToUserId = pr.AssignedToUserId,
                AssignedToUser = null, // Populated in Web layer

                ReviewedByUserId = pr.ReviewedByUserId,
                ReviewedByUser = null, // Populated in Web layer
                ReviewComments = pr.ReviewNotes,

                ApprovedByUserId = pr.ApprovedByUserId,
                ApprovedByUser = null, // Populated in Web layer
                ApprovalNotes = pr.ApprovalNotes,

                DeniedByUserId = null,
                DeniedByUser = null,
                DenialReason = pr.DenialReason,

                ConvertedByUserId = pr.ConvertedByUserId,
                ConvertedByUser = null, // Populated in Web layer

                ProjectId = pr.ProjectId,

                EstimatedBudget = null,
                ProposedStartDate = null,
                ProposedEndDate = null,
                DueDate = pr.DueDate,

                CreatedDate = pr.CreatedDate,
                SubmittedDate = pr.SubmittedDate,
                ReviewedDate = pr.ReviewStartedDate,
                ApprovedDate = pr.ApprovedDate,
                DeniedDate = pr.DeniedDate,
                ConvertedDate = pr.ConvertedDate,
                LastModifiedDate = pr.LastModifiedDate
            }).ToList();

            var response = new ListProjectRequestsResponse(
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
            _logger.LogError(ex, "Error listing project requests");
            return Result.Error("An error occurred while retrieving project requests");
        }
    }
}
