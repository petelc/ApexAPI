using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Cancel;

/// <summary>
/// Command to cancel a request
/// </summary>
public record CancelRequestCommand(RequestId RequestId) : IRequest<Result>;