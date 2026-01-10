using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.ChangeRequests.Reports;
using Apex.API.UseCases.ChangeRequests.Reports.GetChangeMetrics;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.ChangeRequests.Reports;

[HttpGet("/reports/change-metrics")]
[Authorize]
public class GetChangeMetricsEndpoint : Endpoint<GetChangeMetricsRequest, ChangeMetricsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetChangeMetricsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override async Task HandleAsync(GetChangeMetricsRequest req, CancellationToken ct)
    {
        var tenantId = TenantId.From(_currentUserService.TenantId);

        var query = new GetChangeMetricsQuery(
            tenantId,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}

public class GetChangeMetricsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
