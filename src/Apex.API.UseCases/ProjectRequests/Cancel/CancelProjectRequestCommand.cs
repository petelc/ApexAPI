using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Cancel;

/// <summary>
/// Command to cancel a ProjectRequest
/// </summary>
public record CancelProjectRequestCommand(ProjectRequestId ProjectRequestId) : IRequest<Result>;