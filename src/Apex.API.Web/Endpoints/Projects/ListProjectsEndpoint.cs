using Apex.API.UseCases.Projects.List;
using Apex.API.UseCases.Users.Interfaces;
using FastEndpoints;
using MediatR;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// List projects with filtering, pagination, and user information
/// MATCHES your actual ListProjectsQuery parameters
/// </summary>
public class ListProjectsEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public ListProjectsEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/projects");
        AllowAnonymous(); // Or add your auth policy
        
        Description(b => b
            .WithTags("Projects")
            .WithSummary("List projects with filtering and pagination")
            .WithDescription(@"
Returns a paginated list of projects with full user information.

Query Parameters:
- status: Filter by status (e.g., 'Planning', 'Active', 'Completed')
- priority: Filter by priority (e.g., 'Low', 'Medium', 'High', 'Urgent')
- projectManagerUserId: Filter by project manager user ID
- createdByUserId: Filter by creator user ID
- startDate: Filter projects starting after this date
- endDate: Filter projects ending before this date
- pageNumber: Page number (default: 1)
- pageSize: Page size (default: 20, max: 100)

Example: GET /projects?status=Active&priority=High&pageSize=50
"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get query parameters matching YOUR ListProjectsQuery
        var status = Query<string>("status", isRequired: false);
        var priority = Query<string>("priority", isRequired: false);
        var projectManagerUserId = Query<Guid?>("projectManagerUserId", isRequired: false);
        var createdByUserId = Query<Guid?>("createdByUserId", isRequired: false);
        var startDate = Query<DateTime?>("startDate", isRequired: false);
        var endDate = Query<DateTime?>("endDate", isRequired: false);
        var pageNumber = Query<int?>("pageNumber", isRequired: false) ?? 1;
        var pageSize = Query<int?>("pageSize", isRequired: false) ?? 20;

        // Validate page size
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 20;

        // Create query with correct parameter order
        var query = new ListProjectsQuery(
            Status: status,
            Priority: priority,
            ProjectManagerUserId: projectManagerUserId,
            CreatedByUserId: createdByUserId,
            StartDate: startDate,
            EndDate: endDate,
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
        foreach (var dto in response.Projects)
        {
            userIds.Add(dto.CreatedByUserId);
            
            if (dto.ProjectManagerUserId.HasValue)
                userIds.Add(dto.ProjectManagerUserId.Value);
        }

        // Batch lookup all users (single DB query)
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich DTOs with user information
        var enrichedProjects = response.Projects.Select(dto => dto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(dto.CreatedByUserId),
            
            ProjectManagerUser = dto.ProjectManagerUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.ProjectManagerUserId.Value)
                : null
        }).ToList();

        // Create enriched response
        var enrichedResponse = response with
        {
            Projects = enrichedProjects
        };

        await HttpContext.Response.WriteAsJsonAsync(enrichedResponse, ct);
    }
}
