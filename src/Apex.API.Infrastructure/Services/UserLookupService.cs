using Apex.API.UseCases.Users.DTOs;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Apex.API.Infrastructure.Services;

/// <summary>
/// Service for looking up user information with caching
/// </summary>
public class UserLookupService : IUserLookupService
{
    private readonly ApexDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserLookupService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public UserLookupService(
        ApexDbContext context,
        IMemoryCache cache,
        ILogger<UserLookupService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"user_{userId}";
        if (_cache.TryGetValue<UserDto>(cacheKey, out var cachedUser))
        {
            return cachedUser;
        }

        // Query database
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Cache the result (even if null, to avoid repeated DB queries)
        if (user != null)
        {
            _cache.Set(cacheKey, user, CacheDuration);
        }

        return user;
    }

    public async Task<UserSummaryDto?> GetUserSummaryByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"user_summary_{userId}";
        if (_cache.TryGetValue<UserSummaryDto>(cacheKey, out var cachedSummary))
        {
            return cachedSummary;
        }

        // Query database
        var summary = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email ?? string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Cache the result
        if (summary != null)
        {
            _cache.Set(cacheKey, summary, CacheDuration);
        }

        return summary;
    }

    public async Task<Dictionary<Guid, UserDto>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = userIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new Dictionary<Guid, UserDto>();

        var result = new Dictionary<Guid, UserDto>();
        var uncachedIds = new List<Guid>();

        // Check cache for each user
        foreach (var userId in distinctIds)
        {
            var cacheKey = $"user_{userId}";
            if (_cache.TryGetValue<UserDto>(cacheKey, out var cachedUser) && cachedUser != null)
            {
                result[userId] = cachedUser;
            }
            else
            {
                uncachedIds.Add(userId);
            }
        }

        // Fetch uncached users from database
        if (uncachedIds.Any())
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => uncachedIds.Contains(u.Id))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive
                })
                .ToListAsync(cancellationToken);

            // Add to result and cache
            foreach (var user in users)
            {
                result[user.Id] = user;
                var cacheKey = $"user_{user.Id}";
                _cache.Set(cacheKey, user, CacheDuration);
            }
        }

        return result;
    }

    public async Task<Dictionary<Guid, UserSummaryDto>> GetUserSummariesByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = userIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new Dictionary<Guid, UserSummaryDto>();

        var result = new Dictionary<Guid, UserSummaryDto>();
        var uncachedIds = new List<Guid>();

        // Check cache for each user
        foreach (var userId in distinctIds)
        {
            var cacheKey = $"user_summary_{userId}";
            if (_cache.TryGetValue<UserSummaryDto>(cacheKey, out var cachedSummary) && cachedSummary != null)
            {
                result[userId] = cachedSummary;
            }
            else
            {
                uncachedIds.Add(userId);
            }
        }

        // Fetch uncached users from database
        if (uncachedIds.Any())
        {
            var summaries = await _context.Users
                .AsNoTracking()
                .Where(u => uncachedIds.Contains(u.Id))
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email ?? string.Empty
                })
                .ToListAsync(cancellationToken);

            // Add to result and cache
            foreach (var summary in summaries)
            {
                result[summary.Id] = summary;
                var cacheKey = $"user_summary_{summary.Id}";
                _cache.Set(cacheKey, summary, CacheDuration);
            }
        }

        return result;
    }
}
