using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LoggingExample.Web.Services.Cache
{
    /// <summary>
    /// Redis önbellek servisi için implementasyon
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, IConfiguration configuration, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedResponse = await _cache.GetStringAsync(key);

                if (cachedResponse == null)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data from cache for key: {Key}", key);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, int? absoluteExpiration = null, int? slidingExpiration = null) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                // Varsayılan veya yapılandırma dosyasından değerleri al
                int defaultAbsoluteExpiration = int.TryParse(_configuration["Redis:AbsoluteExpirationMinutes"], out int configAbsoluteExpiration) 
                    ? configAbsoluteExpiration 
                    : 60; // Varsayılan 60 dakika
                
                int defaultSlidingExpiration = int.TryParse(_configuration["Redis:SlidingExpirationMinutes"], out int configSlidingExpiration) 
                    ? configSlidingExpiration 
                    : 20; // Varsayılan 20 dakika

                // Belirtilen değerler veya varsayılan değerleri kullan
                if (absoluteExpiration.HasValue || defaultAbsoluteExpiration > 0)
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteExpiration ?? defaultAbsoluteExpiration);
                }

                if (slidingExpiration.HasValue || defaultSlidingExpiration > 0)
                {
                    options.SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration ?? defaultSlidingExpiration);
                }

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
                _logger.LogDebug("Data cached successfully for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data in cache for key: {Key}", key);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("Cache entry removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from cache for key: {Key}", key);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedResponse = await _cache.GetStringAsync(key);
                return cachedResponse != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
                return false;
            }
        }
    }
} 