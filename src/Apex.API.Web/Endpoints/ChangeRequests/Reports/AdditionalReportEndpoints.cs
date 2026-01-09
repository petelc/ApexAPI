using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Apex.API.UseCases.ChangeRequests.Reports;
using Apex.API.UseCases.ChangeRequests.Reports.GetSuccessRate;
using Apex.API.UseCases.ChangeRequests.Reports.GetMonthlyTrends;
using Apex.API.UseCases.ChangeRequests.Reports.GetTopAffectedSystems;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.ChangeRequests.Reports;

// ========== SUCCESS RATE ENDPOINT ==========

[HttpGet("/reports/success-rate")]
[Authorize]
public class GetSuccessRateEndpoint : Endpoint<GetSuccessRateRequest, SuccessRateResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetSuccessRateEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override async Task HandleAsync(GetSuccessRateRequest req, CancellationToken ct)
    {
        var tenantId = TenantId.From(_currentUserService.TenantId);

        var query = new GetSuccessRateQuery(
            tenantId,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}

public class GetSuccessRateRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ========== MONTHLY TRENDS ENDPOINT ==========

[HttpGet("/reports/monthly-trends")]
[Authorize]
public class GetMonthlyTrendsEndpoint : Endpoint<GetMonthlyTrendsRequest, MonthlyTrendsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyTrendsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override async Task HandleAsync(GetMonthlyTrendsRequest req, CancellationToken ct)
    {
        var tenantId = TenantId.From(_currentUserService.TenantId);

        var query = new GetMonthlyTrendsQuery(
            tenantId,
            req.MonthsBack ?? 12);

        var result = await _mediator.Send(query, ct);

        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}

public class GetMonthlyTrendsRequest
{
    public int? MonthsBack { get; set; }
}

// ========== TOP AFFECTED SYSTEMS ENDPOINT ==========

[HttpGet("/reports/top-affected-systems")]
[Authorize]
public class GetTopAffectedSystemsEndpoint : Endpoint<GetTopAffectedSystemsRequest, TopAffectedSystemsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetTopAffectedSystemsEndpoint(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public override async Task HandleAsync(GetTopAffectedSystemsRequest req, CancellationToken ct)
    {
        var tenantId = TenantId.From(_currentUserService.TenantId);

        var query = new GetTopAffectedSystemsQuery(
            tenantId,
            req.TopCount ?? 10,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(query, ct);

        await HttpContext.Response.WriteAsJsonAsync(result, ct);
    }
}

public class GetTopAffectedSystemsRequest
{
    public int? TopCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
