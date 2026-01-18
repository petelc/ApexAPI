using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.Projects.GetById;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Projects.DTOs;

namespace Apex.API.Web.Endpoints.Projects;

/// <summary>
/// Get single project by ID with user information enrichment
/// </summary>
public class GetProjectEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public GetProjectEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/projects/{id}");
        Roles("User");
        Summary(s =>
        {
            s.Summary = "Get project by ID";
            s.Description = "Retrieves a project with all details including user information";
            s.Response<ProjectDto>(200, "Project found");
            s.Response(404, "Project not found");
            s.Response(403, "Forbidden - project belongs to another tenant");
            s.Response(401, "Unauthorized - JWT token required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetProjectByIdQuery(ProjectId.From(id));

        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            return;
        }

        // ✅ USER LOOKUP HAPPENS HERE (Web layer - maintains Clean Architecture!)
        var projectDto = result.Value;

        // Collect user IDs that need lookup
        var userIds = new List<Guid> { projectDto.CreatedByUserId };

        if (projectDto.ProjectManagerUserId.HasValue)
            userIds.Add(projectDto.ProjectManagerUserId.Value);

        // Batch lookup users (single DB query)
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich DTO with user information
        var enrichedDto = projectDto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(projectDto.CreatedByUserId),

            ProjectManagerUser = projectDto.ProjectManagerUserId.HasValue
                ? userLookup.GetValueOrDefault(projectDto.ProjectManagerUserId.Value)
                : null
        };

        await HttpContext.Response.WriteAsJsonAsync(enrichedDto, ct);
    }
}
