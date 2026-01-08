using Hangfire;
using Microsoft.Extensions.Logging;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Infrastructure.Jobs;

/// <summary>
/// Background job to automatically start changes at their scheduled time
/// </summary>
public class AutoStartChangeJob
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ILogger<AutoStartChangeJob> _logger;

    public AutoStartChangeJob(
        IRepository<ChangeRequest> repository,
        ILogger<AutoStartChangeJob> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Check for scheduled changes that should start now and automatically start them
    /// Runs every 5 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task CheckAndStartScheduledChanges()
    {
        _logger.LogInformation("🔄 Checking for changes to auto-start...");

        try
        {
            var now = DateTime.UtcNow;

            // Get all changes
            var allChanges = await _repository.ListAsync();

            // Find changes that are scheduled and past their start time
            var changesToStart = allChanges
                .Where(c => c.Status == ChangeRequestStatus.Scheduled &&
                           c.ScheduledStartDate.HasValue &&
                           c.ScheduledStartDate.Value <= now)
                .ToList();

            if (!changesToStart.Any())
            {
                _logger.LogInformation("✅ No changes need to be started");
                return;
            }

            _logger.LogInformation("🚀 Found {Count} change(s) to auto-start", changesToStart.Count);

            var successCount = 0;
            var failCount = 0;

            foreach (var change in changesToStart)
            {
                try
                {
                    _logger.LogInformation(
                        "▶️ Starting change: {Id} - {Title}",
                        change.Id,
                        change.Title);

                    change.StartExecution();
                    await _repository.UpdateAsync(change);

                    successCount++;

                    _logger.LogInformation(
                        "✅ Successfully started change: {Id}",
                        change.Id);
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(
                        ex,
                        "❌ Error starting change: {Id} - {Title}",
                        change.Id,
                        change.Title);
                }
            }

            _logger.LogInformation(
                "📊 Auto-start summary: {Success} succeeded, {Failed} failed",
                successCount,
                failCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in CheckAndStartScheduledChanges job");
            throw; // Re-throw to let Hangfire handle retry
        }
    }
}
