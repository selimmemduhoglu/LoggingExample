using LoggingExample.Web.Repositories;

namespace LoggingExample.Web.Services.Cache
{
    /// <summary>
    /// Süresi dolmuş önbellek kayıtlarını temizleyen arka plan servisi
    /// </summary>
    public class CacheCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;

        /// <summary>
        /// CacheCleanupService constructor
        /// </summary>
        public CacheCleanupService(
            IServiceProvider serviceProvider,
            ILogger<CacheCleanupService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Temizleme aralığını yapılandırmadan al (varsayılan: 30 dakika)
            int intervalMinutes = 30;
            if (int.TryParse(configuration["Redis:CleanupIntervalMinutes"], out int configInterval))
            {
                intervalMinutes = configInterval;
            }
            
            _cleanupInterval = TimeSpan.FromMinutes(intervalMinutes);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache cleanup service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Cache cleanup task running");
                
                try
                {
                    await CleanupExpiredCacheEntriesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during cache cleanup");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Cache cleanup service is stopping");
        }

        /// <summary>
        /// Süresi dolmuş önbellek kayıtlarını temizler
        /// </summary>
        private async Task CleanupExpiredCacheEntriesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var cachedRequestRepository = scope.ServiceProvider.GetRequiredService<ICachedRequestRepository>();

            // Süresi dolmuş kayıtları sil
            int removedCount = await cachedRequestRepository.CleanupExpiredRequestsAsync();
            
            if (removedCount > 0)
            {
                _logger.LogInformation("Removed {Count} expired cache entries", removedCount);
            }
            else
            {
                _logger.LogDebug("No expired cache entries found");
            }
        }
    }
} 