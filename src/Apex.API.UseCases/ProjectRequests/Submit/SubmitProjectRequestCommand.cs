using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ProjectRequests.Submit;

/// <summary>
/// Command to submit a ProjectRequest for review (Draft → Pending)
/// </summary>
public record SubmitProjectRequestCommand(ProjectRequestId ProjectRequestId) : IRequest<Result>;
