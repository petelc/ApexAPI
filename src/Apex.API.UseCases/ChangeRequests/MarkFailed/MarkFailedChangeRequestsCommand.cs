using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.MarkFailed;

public record MarkFailedChangeRequestCommand(string Reason, ChangeRequestId ChangeRequestId) : IRequest<Result>;