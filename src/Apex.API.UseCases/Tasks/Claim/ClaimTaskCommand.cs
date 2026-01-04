using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Claim;

///<summary>
/// Command for a user to claim a department-assigned task
///</summary>
public record ClaimTaskCommand(TaskId TaskId) : IRequest<Result>;