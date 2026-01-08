using FastEndpoints;
using MediatR;
using Apex.API.UseCases.ChangeRequests.Reports;
using Apex.API.UseCases.ChangeRequests.Reports.GetChangeMetrics;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.ChangeRequests.Reports;

public class GetChangeMetricsEndpoint : Endpoint<GetChangeMetricsRequest, ChangeMetricsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetChangeMetricsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Get("/api/reports/change-metrics");
        Policies("AuthenticatedUser");
    }

    public override async Task HandleAsync(GetChangeMetricsRequest req, CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId!;

        var query = new GetChangeMetricsQuery(
            tenantId,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}

public class GetChangeMetricsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
