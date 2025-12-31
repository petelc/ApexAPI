using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Apex.API.UseCases.ProjectRequests.Approve;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Web.Endpoints.ProjectRequests;

public class ApproveProjectRequestRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// Endpoint for approving a request
/// </summary>
public class ApproveProjectRequestEndpoint : Endpoint<ApproveProjectRequestRequest>
{
    private readonly IMediator _mediator;

    public ApproveProjectRequestEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/project-requests/{id}/approve");
        Roles("TenantAdmin"); // Only TenantAdmin can approve
        Summary(s =>
        {
            s.Summary = "Approve a request";
            s.Description = "Changes request status to Approved (requires TenantAdmin role)";
            s.Response(200, "Request approved successfully");
            s.Response(400, "Cannot approve request in current status");
            s.Response(404, "Request not found");
            s.Response(403, "Forbidden - requires TenantAdmin role");
            s.Response(401, "Unauthorized");
        });
    }

    public override async Task HandleAsync(ApproveProjectRequestRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new ApproveProjectRequestCommand(ProjectRequestId.From(id), req.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await HttpContext.Response.WriteAsJsonAsync(new { Message = "Request approved successfully." }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Ardalis.Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = result.Errors }, ct);
        }
    }
}
