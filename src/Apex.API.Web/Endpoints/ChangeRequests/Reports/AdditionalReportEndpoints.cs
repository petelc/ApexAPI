using FastEndpoints;
using MediatR;
using Apex.API.UseCases.ChangeRequests.Reports;
using Apex.API.UseCases.ChangeRequests.Reports.GetSuccessRate;
using Apex.API.UseCases.ChangeRequests.Reports.GetMonthlyTrends;
using Apex.API.UseCases.ChangeRequests.Reports.GetTopAffectedSystems;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.ChangeRequests.Reports;

// ========== SUCCESS RATE ENDPOINT ==========

public class GetSuccessRateEndpoint : Endpoint<GetSuccessRateRequest, SuccessRateResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetSuccessRateEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Get("/api/reports/success-rate");
        Policies("AuthenticatedUser");
    }

    public override async Task HandleAsync(GetSuccessRateRequest req, CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId!;

        var query = new GetSuccessRateQuery(
            tenantId,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}

public class GetSuccessRateRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ========== MONTHLY TRENDS ENDPOINT ==========

public class GetMonthlyTrendsEndpoint : Endpoint<GetMonthlyTrendsRequest, MonthlyTrendsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyTrendsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Get("/api/reports/monthly-trends");
        Policies("AuthenticatedUser");
    }

    public override async Task HandleAsync(GetMonthlyTrendsRequest req, CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId!;

        var query = new GetMonthlyTrendsQuery(
            tenantId,
            req.MonthsBack ?? 12);

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}

public class GetMonthlyTrendsRequest
{
    public int? MonthsBack { get; set; }
}

// ========== TOP AFFECTED SYSTEMS ENDPOINT ==========

public class GetTopAffectedSystemsEndpoint : Endpoint<GetTopAffectedSystemsRequest, TopAffectedSystemsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetTopAffectedSystemsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Get("/api/reports/top-affected-systems");
        Policies("AuthenticatedUser");
    }

    public override async Task HandleAsync(GetTopAffectedSystemsRequest req, CancellationToken ct)
    {
        var tenantId = _currentUserService.TenantId!;

        var query = new GetTopAffectedSystemsQuery(
            tenantId,
            req.TopCount ?? 10,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}

public class GetTopAffectedSystemsRequest
{
    public int? TopCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
