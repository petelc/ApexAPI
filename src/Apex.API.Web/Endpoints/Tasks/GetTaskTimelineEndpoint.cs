using FastEndpoints;
using MediatR;
using Apex.API.UseCases.Tasks.Timeline;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Endpoint for getting task activity timeline with user information
/// User enrichment happens HERE (Web layer) to maintain Clean Architecture
/// </summary>
public class GetTaskTimelineEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public GetTaskTimelineEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/tasks/{id}/timeline");
        AllowAnonymous(); // Or add your auth policy
        
        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Get task timeline")
            .WithDescription("Returns activity log/audit trail for a task with user information"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");

        var query = new GetTaskTimelineQuery(TaskId.From(taskId));

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

        // ✅ USER ENRICHMENT (Web layer - maintains Clean Architecture!)
        var activities = result.Value;

        // Collect all user IDs
        var userIds = activities
            .Select(a => a.UserId)
            .Distinct()
            .ToList();

        // Batch lookup users
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich with user info
        var enrichedActivities = activities.Select(a =>
        {
            var user = userLookup.GetValueOrDefault(a.UserId);
            return a with
            {
                UserName = user?.FullName,
                UserEmail = user?.Email
            };
        }).ToList();

        await HttpContext.Response.WriteAsJsonAsync(enrichedActivities, ct);
    }
}
