using LoggingExample.Web.Data;
using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace LoggingExample.Web.Repositories
{
    public class CachedRequestRepository : Repository<CachedRequest>, ICachedRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CachedRequestRepository> _logger;

        public CachedRequestRepository(ApplicationDbContext context, ILogger<CachedRequestRepository> logger) 
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<CachedRequest?> GetByCacheKeyAsync(string cacheKey)
        {
            _logger.LogDebug("Getting cached request by cache key: {CacheKey}", cacheKey);
            return await _context.CachedRequests
                .FirstOrDefaultAsync(c => c.CacheKey == cacheKey);
        }
        public async Task IncrementHitCountAsync(string cacheKey)
        {
            _logger.LogDebug("Incrementing hit count for cache key: {CacheKey}", cacheKey);
            var cachedRequest = await GetByCacheKeyAsync(cacheKey);
            
            if (cachedRequest != null)
            {
                cachedRequest.HitCount++;
                cachedRequest.LastAccessed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CachedRequest>> GetByUserIdAsync(int userId)
        {
            _logger.LogDebug("Getting cached requests for user ID: {UserId}", userId);
            return await _context.CachedRequests
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.LastAccessed)
                .ToListAsync();
        }

        public async Task<int> CleanupExpiredRequestsAsync()
        {
            _logger.LogDebug("Cleaning up expired cached requests");
            var now = DateTime.UtcNow;
            var expiredRequests = await _context.CachedRequests
                .Where(c => c.ExpiresAt < now)
                .ToListAsync();
            
            if (expiredRequests.Any())
            {
                _context.CachedRequests.RemoveRange(expiredRequests);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} expired cached requests", expiredRequests.Count);
                return expiredRequests.Count;
            }
            
            return 0;
        }

        public async Task<IEnumerable<CachedRequest>> GetTopHitRequestsAsync(int limit = 10)
        {
            _logger.LogDebug("Getting top {Limit} hit cached requests", limit);
            return await _context.CachedRequests
                .OrderByDescending(c => c.HitCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<CachedRequest>> GetByCorrelationIdAsync(string correlationId)
        {
            _logger.LogDebug("Getting cached requests for correlation ID: {CorrelationId}", correlationId);
            return await _context.CachedRequests
                .Where(c => c.CorrelationId == correlationId)
                .OrderByDescending(c => c.CachedAt)
                .ToListAsync();
        }
    }
} 