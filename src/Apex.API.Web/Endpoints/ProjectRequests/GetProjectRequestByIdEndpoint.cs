using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.ProjectRequests.GetById;
using Apex.API.UseCases.Users.Interfaces;
using Ardalis.Result;
using FastEndpoints;
using MediatR;

namespace Apex.API.Web.Endpoints.ProjectRequests;

/// <summary>
/// Get a single project request by ID with user information
/// User enrichment happens HERE (Web layer) to maintain Clean Architecture
/// </summary>
public class GetProjectRequestByIdEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public GetProjectRequestByIdEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/project-requests/{id}");
        AllowAnonymous(); // Or add your auth policy

        Description(b => b
            .WithTags("Project-Requests")
            .WithSummary("Get a project request by ID with user information")
            .WithDescription("Returns a single project request with full user details."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get project request ID from route
        var projectRequestIdGuid = Route<Guid>("id");

        // ✅ FIXED: Use CreateFrom static method (not constructor)
        var projectRequestId = ProjectRequestId.From(projectRequestIdGuid);

        var query = new GetProjectRequestByIdQuery(projectRequestId);

        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
                return;
            }

        }

        var dto = result.Value;

        // ✅ CRITICAL: Collect all user IDs that need lookup
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

        // ✅ CRITICAL: Batch lookup all users (single DB query)
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // ✅ CRITICAL: Enrich DTO with user information
        var enrichedDto = dto with
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

        // ✅ FIXED: Return enriched DTO with user information
        await HttpContext.Response.WriteAsJsonAsync(enrichedDto, ct);
    }
}
