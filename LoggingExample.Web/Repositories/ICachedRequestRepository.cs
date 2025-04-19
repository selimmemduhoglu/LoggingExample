using LoggingExample.Web.Models.Entity;

namespace LoggingExample.Web.Repositories
{
    /// <summary>
    /// CachedRequest için repository interface'i
    /// </summary>
    public interface ICachedRequestRepository : IRepository<CachedRequest>
    {
        /// <summary>
        /// Cache key'e göre önbellek isteğini getirir
        /// </summary>
        /// <param name="cacheKey">Önbellek anahtarı</param>
        /// <returns>Bulunan önbellek kaydı veya null</returns>
        Task<CachedRequest?> GetByCacheKeyAsync(string cacheKey);
        
        /// <summary>
        /// Önbellek kaydının hit sayısını artırır
        /// </summary>
        /// <param name="cacheKey">Önbellek anahtarı</param>
        /// <returns>İşlem sonucu</returns>
        Task IncrementHitCountAsync(string cacheKey);
        
        /// <summary>
        /// Kullanıcıya ait önbellek isteklerini getirir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>Önbellek istekleri listesi</returns>
        Task<IEnumerable<CachedRequest>> GetByUserIdAsync(int userId);
        
        /// <summary>
        /// Süresi dolmuş önbellek isteklerini temizler
        /// </summary>
        /// <returns>Silinen kayıt sayısı</returns>
        Task<int> CleanupExpiredRequestsAsync();
        
        /// <summary>
        /// En çok hit alan önbellek isteklerini getirir
        /// </summary>
        /// <param name="limit">Getirilecek kayıt sayısı</param>
        /// <returns>Önbellek istekleri listesi</returns>
        Task<IEnumerable<CachedRequest>> GetTopHitRequestsAsync(int limit = 10);
        
        /// <summary>
        /// Belirli bir korelasyon ID'sine ait önbellek isteklerini getirir
        /// </summary>
        /// <param name="correlationId">Korelasyon ID</param>
        /// <returns>Önbellek istekleri listesi</returns>
        Task<IEnumerable<CachedRequest>> GetByCorrelationIdAsync(string correlationId);
    }
} 