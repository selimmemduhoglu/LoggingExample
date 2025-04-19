using LoggingExample.Web.Models.Entity;
using LoggingExample.Web.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
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
        private readonly ICachedRequestRepository? _cachedRequestRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedisCacheService(
            IDistributedCache cache, 
            IConfiguration configuration, 
            ILogger<RedisCacheService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICachedRequestRepository? cachedRequestRepository = null)
        {
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _cachedRequestRepository = cachedRequestRepository;
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
                
                // Veritabanı izlemesi etkinse, hit sayısını artır
                await TrackCacheHitAsync(key);
                
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
                
                // Önbellek isteğini veritabanında izle
                await TrackCacheAddAsync(key, serializedValue, options);
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
                
                // Veritabanı izleme etkinse, kaydı veritabanından da sil
                await RemoveCacheEntryFromDatabaseAsync(key);
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
        
        #region Database Tracking
        
        /// <summary>
        /// Önbellek eklemesini veritabanında izler
        /// </summary>
        private async Task TrackCacheAddAsync(string key, string serializedValue, DistributedCacheEntryOptions options)
        {
            if (_cachedRequestRepository == null)
                return;
            
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return;
                
                // Mevcut istek URL ve HTTP metodu bilgisi
                var requestUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
                var httpMethod = httpContext.Request.Method;
                
                // Korelasyon ID
                var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? 
                                   Guid.NewGuid().ToString();
                
                // Kullanıcı ID (Eğer giriş yapmış bir kullanıcı varsa)
                int? userId = null;
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid");
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }
                
                // Önbellek son kullanma tarihini hesapla
                var expiresAt = DateTime.UtcNow;
                if (options.AbsoluteExpiration.HasValue)
                {
                    expiresAt = options.AbsoluteExpiration.Value.DateTime;
                }
                else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    expiresAt = DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
                }
                
                // Mevcut kayıt varsa güncelle, yoksa yeni kayıt oluştur
                var existingEntry = await _cachedRequestRepository.GetByCacheKeyAsync(key);
                if (existingEntry != null)
                {
                    existingEntry.LastAccessed = DateTime.UtcNow;
                    existingEntry.ExpiresAt = expiresAt;
                    existingEntry.DataSize = Encoding.UTF8.GetByteCount(serializedValue);
                    await _cachedRequestRepository.UpdateAsync(existingEntry);
                }
                else
                {
                    var cachedRequest = new CachedRequest
                    {
                        CacheKey = key,
                        RequestUrl = requestUrl,
                        HttpMethod = httpMethod,
                        CachedAt = DateTime.UtcNow,
                        LastAccessed = DateTime.UtcNow,
                        DataSize = Encoding.UTF8.GetByteCount(serializedValue),
                        HitCount = 0,
                        ExpiresAt = expiresAt,
                        UserId = userId,
                        CorrelationId = correlationId
                    };
                    
                    await _cachedRequestRepository.AddAsync(cachedRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error tracking cache entry in database for key: {Key}", key);
            }
        }
        
        /// <summary>
        /// Önbellek hit sayısını veritabanında artırır
        /// </summary>
        private async Task TrackCacheHitAsync(string key)
        {
            if (_cachedRequestRepository == null)
                return;
                
            try
            {
                await _cachedRequestRepository.IncrementHitCountAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error tracking cache hit in database for key: {Key}", key);
            }
        }
        
        /// <summary>
        /// Önbellek kaydını veritabanından siler
        /// </summary>
        private async Task RemoveCacheEntryFromDatabaseAsync(string key)
        {
            if (_cachedRequestRepository == null)
                return;
                
            try
            {
                var cachedRequest = await _cachedRequestRepository.GetByCacheKeyAsync(key);
                if (cachedRequest != null)
                {
                    await _cachedRequestRepository.DeleteAsync(cachedRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error removing cache entry from database for key: {Key}", key);
            }
        }
        
        #endregion
    }
} 