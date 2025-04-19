using LoggingExample.Web.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace LoggingExample.Web.Attributes
{
    /// <summary>
    /// Controller action'larını önbelleklemek için kullanılan attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : ActionFilterAttribute
    {
        private readonly int? _absoluteExpirationMinutes;
        private readonly int? _slidingExpirationMinutes;
        private readonly string[] _varyByQueryParams;
        private readonly string[] _varyByHeaders;
        private readonly bool _varyByUser;

        /// <summary>
        /// Cache attribute constructor
        /// </summary>
        /// <param name="absoluteExpirationMinutes">Mutlak son kullanma süresi (dakika)</param>
        /// <param name="slidingExpirationMinutes">Kayar son kullanma süresi (dakika)</param>
        /// <param name="varyByQueryParams">Sorgu parametreleri için değişkenlik</param>
        /// <param name="varyByHeaders">HTTP başlıkları için değişkenlik</param>
        /// <param name="varyByUser">Kullanıcıya göre değişkenlik</param>
        public CacheAttribute(
            int? absoluteExpirationMinutes = null, 
            int? slidingExpirationMinutes = null,
            string[]? varyByQueryParams = null,
            string[]? varyByHeaders = null,
            bool varyByUser = false)
        {
            _absoluteExpirationMinutes = absoluteExpirationMinutes;
            _slidingExpirationMinutes = slidingExpirationMinutes;
            _varyByQueryParams = varyByQueryParams ?? Array.Empty<string>();
            _varyByHeaders = varyByHeaders ?? Array.Empty<string>();
            _varyByUser = varyByUser;
        }

        /// <summary>
        /// Action çalışmadan önce çalışır. Önbellek kontrolü yapar.
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            
            // Cache key oluştur
            var cacheKey = GenerateCacheKey(context);
            
            // Önbellekten veri kontrolü
            var cachedResult = await cacheService.GetAsync<IActionResult>(cacheKey);
            
            if (cachedResult != null)
            {
                // Önbellekten geldiğini belirtmek için header ekle
                context.HttpContext.Response.Headers.Add("X-Cache", "Hit");
                
                // Önbellekteki sonucu döndür
                context.Result = cachedResult;
                return;
            }
            
            // Önbellekte yoksa, action'ı çalıştır
            context.HttpContext.Response.Headers.Add("X-Cache", "Miss");
            var executedContext = await next();
            
            // Action sonucunu önbelleğe ekle (sadece başarılı sonuçlar için)
            if (executedContext.Result != null && executedContext.Exception == null)
            {
                await cacheService.SetAsync(
                    cacheKey, 
                    executedContext.Result, 
                    _absoluteExpirationMinutes, 
                    _slidingExpirationMinutes);
            }
        }

        /// <summary>
        /// Cache key oluşturur
        /// </summary>
        private string GenerateCacheKey(ActionExecutingContext context)
        {
            var keyBuilder = new StringBuilder();
            
            // Controller ve Action adını ekle
            keyBuilder.Append($"{context.RouteData.Values["controller"]}_{context.RouteData.Values["action"]}");
            
            // Seçili sorgu parametrelerini ekle
            if (_varyByQueryParams.Length > 0)
            {
                foreach (var queryParam in _varyByQueryParams)
                {
                    if (context.HttpContext.Request.Query.TryGetValue(queryParam, out var value))
                    {
                        keyBuilder.Append($"_{queryParam}={value}");
                    }
                }
            }
            
            // Seçili HTTP başlıklarını ekle
            if (_varyByHeaders.Length > 0)
            {
                foreach (var header in _varyByHeaders)
                {
                    if (context.HttpContext.Request.Headers.TryGetValue(header, out var value))
                    {
                        keyBuilder.Append($"_{header}={value}");
                    }
                }
            }
            
            // Kullanıcıya göre değişkenlik
            if (_varyByUser && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.HttpContext.User.FindFirst("sub")?.Value ?? 
                             context.HttpContext.User.FindFirst("nameid")?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    keyBuilder.Append($"_user={userId}");
                }
            }
            
            return $"cache:{keyBuilder.ToString()}";
        }
    }
} 