using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Requests.Submit;

/// <summary>
/// Command to submit a request for review (Draft → Pending)
/// </summary>
public record SubmitRequestCommand(RequestId RequestId) : IRequest<Result>;
