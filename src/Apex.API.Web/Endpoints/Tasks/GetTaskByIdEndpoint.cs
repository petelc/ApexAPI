using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Tasks.GetById;
using Apex.API.UseCases.Users.Interfaces;
using Ardalis.Result;
using FastEndpoints;
using MediatR;

namespace Apex.API.Web.Endpoints.Tasks;

/// <summary>
/// Get a single task by ID with user information
/// User enrichment happens HERE (Web layer) to maintain Clean Architecture
/// </summary>
public class GetTaskByIdEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IUserLookupService _userLookupService;

    public GetTaskByIdEndpoint(
        IMediator mediator,
        IUserLookupService userLookupService)
    {
        _mediator = mediator;
        _userLookupService = userLookupService;
    }

    public override void Configure()
    {
        Get("/tasks/{taskId}");
        AllowAnonymous(); // Or add your auth policy

        Description(b => b
            .WithTags("Tasks")
            .WithSummary("Get a task by ID with user information")
            .WithDescription("Returns a single task with full user details (creator and assignee)."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get task ID from route and convert to TaskId value object
        var id = Route<Guid>("taskId");


        var query = new GetTaskByIdQuery(TaskId.From(id));
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
            }
            return;
        }

        // ✅ USER LOOKUP HAPPENS HERE (Web layer - maintains Clean Architecture!)
        var dto = result.Value;

        // Collect user IDs
        var userIds = new List<Guid> { dto.CreatedByUserId };

        if (dto.AssignedToUserId.HasValue)
            userIds.Add(dto.AssignedToUserId.Value);

        // Batch lookup users
        var userLookup = await _userLookupService.GetUserSummariesByIdsAsync(userIds, ct);

        // Enrich DTO with user information
        var enrichedDto = dto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(dto.CreatedByUserId),

            AssignedToUser = dto.AssignedToUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.AssignedToUserId.Value)
                : null
        };

        await HttpContext.Response.WriteAsJsonAsync(enrichedDto, ct);
    }
}
