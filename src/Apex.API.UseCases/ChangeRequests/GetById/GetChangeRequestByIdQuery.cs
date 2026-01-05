using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.GetById;

/// <summary>
/// Query to get a ChangeRequest by ID
/// </summary>
public record GetChangeRequestByIdQuery(ChangeRequestId ChangeRequestId)
    : IRequest<Result<ChangeRequestDto>>;


