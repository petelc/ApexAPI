using Apex.API.UseCases.ProjectRequests.DTOs;
using Apex.API.UseCases.Users.DTOs;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;

namespace Apex.API.Web.Extensions;

/// <summary>
/// Mapping extensions for ProjectRequest - ENHANCED with all aggregate properties
/// </summary>
public static class ProjectRequestMappingExtensions
{
    /// <summary>
    /// Maps ProjectRequest aggregate to DTO - includes ALL available properties
    /// </summary>
    public static ProjectRequestDto ToDto(this ProjectRequest request)
    {
        return new ProjectRequestDto
        {
            // Value Object → Guid
            Id = request.Id.Value,
            
            // Simple properties
            Title = request.Title,
            Description = request.Description,
            BusinessJustification = string.Empty, // Not in aggregate
            
            // Smart Enums → String
            Status = request.Status.Name,
            Priority = request.Priority.Name,
            
            // User tracking - Requesting User (Creator)
            RequestingUserId = request.CreatedByUserId,
            RequestingUser = null, // Populated by user lookup
            
            // User tracking - Assigned To (NEW!)
            AssignedToUserId = request.AssignedToUserId,
            AssignedToUser = null, // Populated by user lookup
            
            // User tracking - Reviewed By
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedByUser = null, // Populated by user lookup
            ReviewComments = request.ReviewNotes,
            
            // User tracking - Approved By (NEW!)
            ApprovedByUserId = request.ApprovedByUserId,
            ApprovedByUser = null, // Populated by user lookup
            ApprovalNotes = request.ApprovalNotes,
            
            // User tracking - Denied By
            DeniedByUserId = null, // Not tracked in aggregate
            DeniedByUser = null,
            DenialReason = request.DenialReason,
            
            // User tracking - Converted By (NEW!)
            ConvertedByUserId = request.ConvertedByUserId,
            ConvertedByUser = null, // Populated by user lookup
            
            // Project link - Already Guid? (not a value object)
            ProjectId = request.ProjectId,
            
            // Budget & Timeline
            EstimatedBudget = null, // Not in aggregate
            ProposedStartDate = null, // Not in aggregate
            ProposedEndDate = null, // Not in aggregate
            DueDate = request.DueDate, // NEW!
            
            // Dates
            CreatedDate = request.CreatedDate,
            SubmittedDate = request.SubmittedDate,
            ReviewedDate = request.ReviewStartedDate,
            ApprovedDate = request.ApprovedDate, // NEW!
            DeniedDate = request.DeniedDate,
            ConvertedDate = request.ConvertedDate, // NEW!
            LastModifiedDate = request.LastModifiedDate
        };
    }

    /// <summary>
    /// Enriches ProjectRequestDto with user information
    /// </summary>
    public static ProjectRequestDto WithUserInfo(
        this ProjectRequestDto dto,
        Dictionary<Guid, UserSummaryDto> userLookup)
    {
        return dto with
        {
            RequestingUser = userLookup.GetValueOrDefault(dto.RequestingUserId),
            
            AssignedToUser = dto.AssignedToUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.AssignedToUserId.Value)
                : null,
            
            ReviewedByUser = dto.ReviewedByUserId.HasValue 
                ? userLookup.GetValueOrDefault(dto.ReviewedByUserId.Value) 
                : null,
            
            ApprovedByUser = dto.ApprovedByUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.ApprovedByUserId.Value)
                : null,
            
            DeniedByUser = dto.DeniedByUserId.HasValue 
                ? userLookup.GetValueOrDefault(dto.DeniedByUserId.Value) 
                : null,
            
            ConvertedByUser = dto.ConvertedByUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.ConvertedByUserId.Value)
                : null
        };
    }

    /// <summary>
    /// Maps multiple ProjectRequests to DTOs with user information (batch operation)
    /// </summary>
    public static async Task<List<ProjectRequestDto>> ToDtosWithUsersAsync(
        this IEnumerable<ProjectRequest> requests,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken = default)
    {
        var requestsList = requests.ToList();
        if (!requestsList.Any())
            return new List<ProjectRequestDto>();

        // Convert to DTOs
        var dtos = requestsList.Select(r => r.ToDto()).ToList();

        // Collect all user IDs that need lookup
        var userIds = new HashSet<Guid>();
        foreach (var dto in dtos)
        {
            userIds.Add(dto.RequestingUserId);
            
            if (dto.AssignedToUserId.HasValue)
                userIds.Add(dto.AssignedToUserId.Value);
            
            if (dto.ReviewedByUserId.HasValue)
                userIds.Add(dto.ReviewedByUserId.Value);
            
            if (dto.ApprovedByUserId.HasValue)
                userIds.Add(dto.ApprovedByUserId.Value);
            
            if (dto.DeniedByUserId.HasValue)
                userIds.Add(dto.DeniedByUserId.Value);
            
            if (dto.ConvertedByUserId.HasValue)
                userIds.Add(dto.ConvertedByUserId.Value);
        }

        // Batch lookup all users (single DB query)
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        // Enrich DTOs with user information
        return dtos.Select(dto => dto.WithUserInfo(userLookup)).ToList();
    }

    /// <summary>
    /// Maps single ProjectRequest to DTO with user information
    /// </summary>
    public static async Task<ProjectRequestDto> ToDtoWithUsersAsync(
        this ProjectRequest request,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken = default)
    {
        var dto = request.ToDto();

        // Collect user IDs
        var userIds = new List<Guid> { dto.RequestingUserId };
        
        if (dto.AssignedToUserId.HasValue)
            userIds.Add(dto.AssignedToUserId.Value);
        
        if (dto.ReviewedByUserId.HasValue)
            userIds.Add(dto.ReviewedByUserId.Value);
        
        if (dto.ApprovedByUserId.HasValue)
            userIds.Add(dto.ApprovedByUserId.Value);
        
        if (dto.DeniedByUserId.HasValue)
            userIds.Add(dto.DeniedByUserId.Value);
        
        if (dto.ConvertedByUserId.HasValue)
            userIds.Add(dto.ConvertedByUserId.Value);

        // Lookup users
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        // Enrich with user info
        return dto.WithUserInfo(userLookup);
    }
}
