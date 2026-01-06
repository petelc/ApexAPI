using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Cancel;

/// <summary>
/// Command to cancel a ChangeRequest
/// </summary>
public record CancelChangeRequestCommand(ChangeRequestId ChangeRequestId) : IRequest<Result>;