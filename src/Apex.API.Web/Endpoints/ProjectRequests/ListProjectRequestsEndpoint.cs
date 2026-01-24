using Apex.API.UseCases.ProjectRequests.List;
using Apex.API.UseCases.Users.Interfaces;
using FastEndpoints;
using MediatR;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// List project requests with filtering, pagination, and user information
/// </summary>
public class ListProjectRequestsEndpoint : EndpointWithoutRequest<ListProjectRequestsResponse>
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public ListProjectRequestsEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/project-requests");
        AllowAnonymous(); // Or add your auth policy

        Description(b => b
            .WithTags("Project-Requests")
            .WithSummary("List project requests with filtering and pagination")
            .WithDescription(@"
                Returns a paginated list of project requests with full user information.

                Query Parameters:
                - status: Filter by status (e.g., 'Draft', 'Pending', 'Approved')
                - priority: Filter by priority (e.g., 'Low', 'Medium', 'High')
                - assignedToUserId: Filter by assigned user ID
                - createdByUserId: Filter by creator user ID
                - isOverdue: Filter overdue requests (true/false)
                - pageNumber: Page number (default: 1)
                - pageSize: Page size (default: 20, max: 100)

                Example: GET /project-requests?status=Pending&priority=High&pageSize=50
                "));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get query parameters
        var status = Query<string>("status", isRequired: false);
        var priority = Query<string>("priority", isRequired: false);
        var assignedToUserId = Query<Guid?>("assignedToUserId", isRequired: false);
        var createdByUserId = Query<Guid?>("createdByUserId", isRequired: false);
        var isOverdue = Query<bool?>("isOverdue", isRequired: false);
        var pageNumber = Query<int?>("pageNumber", isRequired: false) ?? 1;
        var pageSize = Query<int?>("pageSize", isRequired: false) ?? 20;

        // Validate page size
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 20;

        // Send query to handler (UseCases layer)
        var query = new ListProjectRequestsQuery(
            status,
            priority,
            assignedToUserId,
            createdByUserId,
            isOverdue,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            return;
        }

        // ✅ USER LOOKUP HAPPENS HERE (Web layer - maintains Clean Architecture!)
        var response = result.Value;

        // Collect all user IDs from the DTOs
        var userIds = new HashSet<Guid>();
        foreach (var dto in response.ProjectRequests)
        {
            userIds.Add(dto.RequestingUserId);

            if (dto.AssignedToUserId.HasValue)
                userIds.Add(dto.AssignedToUserId.Value);

            if (dto.ReviewedByUserId.HasValue)
                userIds.Add(dto.ReviewedByUserId.Value);

            if (dto.ApprovedByUserId.HasValue)
                userIds.Add(dto.ApprovedByUserId.Value);

            if (dto.ConvertedByUserId.HasValue)
                userIds.Add(dto.ConvertedByUserId.Value);
        }

        // Batch lookup all users (single DB query)
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich DTOs with user information
        var enrichedRequests = response.ProjectRequests.Select(dto => dto with
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

            ConvertedByUser = dto.ConvertedByUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.ConvertedByUserId.Value)
                : null
        }).ToList();

        // Create enriched response
        var enrichedResponse = response with
        {
            ProjectRequests = enrichedRequests
        };

        await HttpContext.Response.WriteAsJsonAsync(enrichedResponse, ct);
    }
}
