using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Complete;

public record CompleteChangeRequestCommand(string ImplementationNotes, ChangeRequestId ChangeRequestId) : IRequest<Result>;