using LoggingExample.Web.Attributes;
using LoggingExample.Web.Models;
using LoggingExample.Web.Models.Entity;
using LoggingExample.Web.Repositories;
using LoggingExample.Web.Services.Cache;
using Microsoft.AspNetCore.Mvc;

namespace LoggingExample.Web.Controllers
{
    /// <summary>
    /// Redis önbellek kullanımını gösteren örnek controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CacheExampleController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ICachedRequestRepository _cachedRequestRepository;
        private readonly ILogger<CacheExampleController> _logger;

        /// <summary>
        /// Controller constructor
        /// </summary>
        public CacheExampleController(
            ICacheService cacheService,
            ICachedRequestRepository cachedRequestRepository,
            ILogger<CacheExampleController> logger)
        {
            _cacheService = cacheService;
            _cachedRequestRepository = cachedRequestRepository;
            _logger = logger;
        }

        /// <summary>
        /// Cache attribute ile önbelleklenen endpoint
        /// </summary>
        /// <remarks>
        /// Bu endpoint, Cache attribute kullanımını gösterir.
        /// Veri 2 dakika boyunca önbellekte saklanır.
        /// </remarks>
        [HttpGet("time")]
        [Cache(2)]
        public IActionResult GetTime()
        {
            _logger.LogInformation("GetTime endpoint called");
            
            return Ok(new
            {
                CurrentTime = DateTime.Now,
                Message = "Bu veri 2 dakika boyunca önbellekte saklanır",
                GeneratedAt = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        /// <summary>
        /// Query parametreleri ile önbellekleme örneği
        /// </summary>
        /// <remarks>
        /// Bu endpoint, query parametrelerine bağlı olarak farklı önbellek anahtarları oluşturur.
        /// Örneğin /api/cacheexample/data?id=1 ve /api/cacheexample/data?id=2 farklı önbelleklere sahip olacaktır.
        /// </remarks>
        [HttpGet("data")]
        [Cache(absoluteExpirationMinutes: 5, varyByQueryParams: new[] { "id" })]
        public IActionResult GetData([FromQuery] int id)
        {
            _logger.LogInformation("GetData endpoint called with id: {Id}", id);
            
            // Normalize edilmiş bir hesaplama yaparak her ID için farklı bir süre bekletiyor gibi simüle ediyoruz
            Thread.Sleep(500 + (id % 5) * 200);
            
            return Ok(new
            {
                Id = id,
                Data = $"Veri {id}",
                ComplexCalculation = $"ID {id} için kompleks hesaplama sonucu",
                GeneratedAt = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        /// <summary>
        /// Manuel önbellekleme örneği
        /// </summary>
        /// <remarks>
        /// Bu endpoint, ICacheService'i kullanarak manuel olarak önbellekleme yapar.
        /// Hem veriyi önbellekler hem de daha sonra erişim için kullanır.
        /// </remarks>
        [HttpGet("manual/{id}")]
        public async Task<IActionResult> GetManualCachedData(int id)
        {
            string cacheKey = $"manual_data_{id}";
            
            // Önbellekte veri var mı kontrol et
            var cachedData = await _cacheService.GetAsync<object>(cacheKey);
            
            if (cachedData != null)
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                // Önbellekten gelen veriyi döndür
                Response.Headers.Add("X-Cache", "Hit");
                return Ok(cachedData);
            }
            
            // Önbellekte yoksa veriyi oluştur
            _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
            Response.Headers.Add("X-Cache", "Miss");
            
            // Normalize edilmiş bir hesaplama yaparak her ID için farklı bir süre bekletiyor gibi simüle ediyoruz
            Thread.Sleep(800 + (id % 4) * 300);
            
            var data = new
            {
                Id = id,
                Data = $"Manuel önbelleklenmiş veri {id}",
                DetailedInfo = new
                {
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now,
                    ExpiresIn = "10 minutes"
                },
                GeneratedAt = DateTime.Now.ToString("HH:mm:ss")
            };
            
            // Veriyi önbelleğe ekle (10 dakika mutlak, 5 dakika kayar son kullanma süresi)
            await _cacheService.SetAsync(cacheKey, data, absoluteExpiration: 10, slidingExpiration: 5);
            
            return Ok(data);
        }

        /// <summary>
        /// Önbellek istatistiklerini görüntüle
        /// </summary>
        /// <remarks>
        /// Bu endpoint, önbelleğe alınan istekleri ve istatistiklerini gösterir.
        /// </remarks>
        [HttpGet("stats")]
        public async Task<IActionResult> GetCacheStats()
        {
            _logger.LogInformation("GetCacheStats endpoint called");
            
            // En çok hit alan istekleri getir
            var topHits = await _cachedRequestRepository.GetTopHitRequestsAsync(5);
            
            // Toplam istek sayısı
            var totalCount = await _cachedRequestRepository.CountAsync();
            
            return Ok(new
            {
                TotalCachedRequests = totalCount,
                TopHitRequests = topHits.Select(r => new
                {
                    r.CacheKey,
                    r.RequestUrl,
                    r.HttpMethod,
                    r.HitCount,
                    r.CachedAt,
                    r.LastAccessed,
                    r.ExpiresAt,
                    DataSizeKB = Math.Round((double)r.DataSize / 1024, 2)
                })
            });
        }

        /// <summary>
        /// Önbelleği temizle
        /// </summary>
        /// <remarks>
        /// Bu endpoint, belirtilen bir anahtara sahip önbelleği temizler.
        /// Anahtar belirtilmezse tüm zaman tabanlı anahtarları temizler.
        /// </remarks>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCache([FromQuery] string? key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _logger.LogInformation("Clearing cache for key: {Key}", key);
                await _cacheService.RemoveAsync(key);
                return Ok(new { Message = $"Cache cleared for key: {key}" });
            }
            
            // Sadece zaman tabanlı önbellekleri temizle
            _logger.LogInformation("Clearing all time-based cache entries");
            await _cacheService.RemoveAsync("cache:CacheExample_GetTime");
            
            // Tüm data anahtarlarını bul ve temizle
            var dataRequests = await _cachedRequestRepository.FindAsync(c => c.CacheKey.Contains("cache:CacheExample_GetData"));
            foreach (var request in dataRequests)
            {
                await _cacheService.RemoveAsync(request.CacheKey);
            }
            
            return Ok(new { Message = "All time-based cache entries cleared" });
        }
    }
} 