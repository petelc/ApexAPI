using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.StartExecution;

public record StartExecutionChangeRequestCommand(ChangeRequestId ChangeRequestId) : IRequest<Result>;