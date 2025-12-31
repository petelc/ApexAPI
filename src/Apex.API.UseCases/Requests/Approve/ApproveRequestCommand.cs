using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Approve;

/// <summary>
/// Command to approve a request (InReview/Pending → Approved)
/// </summary>
public record ApproveRequestCommand(
    RequestId RequestId,
    string? Notes = null
) : IRequest<Result>;
