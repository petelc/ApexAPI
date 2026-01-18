using Apex.API.UseCases.Tasks.List;
using Apex.API.UseCases.Users.Interfaces;
using FastEndpoints;
using MediatR;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// List tasks for a project with user information
/// User enrichment happens HERE (Web layer) to maintain Clean Architecture
/// </summary>
public class ListTasksEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public ListTasksEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/projects/{projectId}/tasks");
        AllowAnonymous(); // Or add your auth policy
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("List tasks for a project with pagination and user information")
            .WithDescription(@"
Returns a paginated list of tasks for a specific project with full user information.

Query Parameters:
- pageNumber: Page number (default: 1)
- pageSize: Page size (default: 100, max: 100)

Example: GET /projects/{projectId}/tasks?pageNumber=1&pageSize=50
"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get project ID from route
        var projectId = Route<Guid>("projectId");
        
        // Get query parameters
        var pageNumber = Query<int?>("pageNumber", isRequired: false) ?? 1;
        var pageSize = Query<int?>("pageSize", isRequired: false) ?? 100;

        // Validate page size
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 100;

        // Create query matching YOUR ListTasksQuery constructor
        var query = new ListTasksQuery(
            ProjectId: projectId,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

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
        foreach (var dto in response.Tasks)
        {
            // CreatedBy (always has value)
            userIds.Add(dto.CreatedByUserId);
            
            // AssignedTo (might be null)
            if (dto.AssignedToUserId.HasValue)
                userIds.Add(dto.AssignedToUserId.Value);
        }

        // Batch lookup all users (single DB query)
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich DTOs with user information
        var enrichedTasks = response.Tasks.Select(dto => dto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(dto.CreatedByUserId),
            
            AssignedToUser = dto.AssignedToUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.AssignedToUserId.Value)
                : null
        }).ToList();

        // Create enriched response
        var enrichedResponse = response with
        {
            Tasks = enrichedTasks
        };

        await HttpContext.Response.WriteAsJsonAsync(enrichedResponse, ct);
    }
}
