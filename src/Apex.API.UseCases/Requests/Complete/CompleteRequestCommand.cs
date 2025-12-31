using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Complete;

/// <summary>
/// Command to complete a request
/// </summary>
public record CompleteRequestCommand(RequestId RequestId) : IRequest<Result>;