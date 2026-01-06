using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.ChangeRequests.Schedule;

public record ScheduleChangeRequestCommand(DateTime ScheduleStartDate, DateTime ScheduleEndDate, string Window, ChangeRequestId ChangeRequestId) : IRequest<Result>;