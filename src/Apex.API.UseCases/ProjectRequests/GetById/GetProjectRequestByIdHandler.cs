using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.UseCases.ProjectRequests.DTOs;
using Ardalis.Result;
using Traxs.SharedKernel;
using MediatR;

namespace Apex.API.UseCases.ProjectRequests.GetById;

/// <summary>
/// Handler for GetProjectRequestByIdQuery
/// Returns DTO with user IDs only (no user objects)
/// User enrichment happens at Web layer
/// </summary>
public class GetProjectRequestByIdHandler : IRequestHandler<GetProjectRequestByIdQuery, Result<ProjectRequestDto>>
{
    private readonly IReadRepository<ProjectRequest> _repository;

    public GetProjectRequestByIdHandler(IReadRepository<ProjectRequest> repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProjectRequestDto>> Handle(
        GetProjectRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Fetch project request by ID
        var projectRequest = await _repository.GetByIdAsync(request.ProjectRequestId, cancellationToken);

        if (projectRequest == null)
            return Result.NotFound("Project request not found");

        // Map to DTO (user objects are null - enriched at Web layer)
        // ✅ Using object initializer syntax (not positional)
        var dto = new ProjectRequestDto
        {
            Id = projectRequest.Id.Value,
            Title = projectRequest.Title,
            Description = projectRequest.Description,
            BusinessJustification = projectRequest.BusinessJustification, // Not in aggregate
            Status = projectRequest.Status.Name,
            Priority = projectRequest.Priority.Name,

            // User IDs (objects populated at Web layer)
            RequestingUserId = projectRequest.CreatedByUserId,
            RequestingUser = null,

            AssignedToUserId = projectRequest.AssignedToUserId,
            AssignedToUser = null,

            ReviewedByUserId = projectRequest.ReviewedByUserId,
            ReviewedByUser = null,

            ApprovedByUserId = projectRequest.ApprovedByUserId,
            ApprovedByUser = null,

            DeniedByUserId = null, // Not tracked in aggregate
            DeniedByUser = null,

            ConvertedByUserId = projectRequest.ConvertedByUserId,
            ConvertedByUser = null,

            // Project link
            ProjectId = projectRequest.ProjectId,

            // Budget & Timeline
            EstimatedBudget = projectRequest.EstimatedBudget, // Not in aggregate
            ProposedStartDate = projectRequest.ProposedStartDate, // Not in aggregate
            ProposedEndDate = projectRequest.ProposedEndDate, // Not in aggregate
            DueDate = projectRequest.DueDate,

            // Dates
            CreatedDate = projectRequest.CreatedDate,
            SubmittedDate = projectRequest.SubmittedDate,
            ReviewedDate = projectRequest.ReviewStartedDate,
            ApprovedDate = projectRequest.ApprovedDate,
            DeniedDate = projectRequest.DeniedDate,
            ConvertedDate = projectRequest.ConvertedDate,
            LastModifiedDate = projectRequest.LastModifiedDate,

            // Notes
            ReviewComments = projectRequest.ReviewNotes,
            ApprovalNotes = projectRequest.ApprovalNotes,
            DenialReason = projectRequest.DenialReason
        };

        return Result.Success(dto);
    }
}
