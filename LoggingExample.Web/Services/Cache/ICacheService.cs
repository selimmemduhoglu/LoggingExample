namespace LoggingExample.Web.Services.Cache
{
    /// <summary>
    /// Cache servisi için interface
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Cache'den veri almak için kullanılır
        /// </summary>
        /// <typeparam name="T">Dönüş tipi</typeparam>
        /// <param name="key">Cache anahtarı</param>
        /// <returns>Cache'de bulunan veri veya null</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Cache'e veri eklemek için kullanılır
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="key">Cache anahtarı</param>
        /// <param name="value">Cache'e eklenecek veri</param>
        /// <param name="absoluteExpiration">Mutlak son kullanma süresi (dakika)</param>
        /// <param name="slidingExpiration">Kayar son kullanma süresi (dakika)</param>
        /// <returns>İşlem sonucu</returns>
        Task SetAsync<T>(string key, T value, int? absoluteExpiration = null, int? slidingExpiration = null) where T : class;

        /// <summary>
        /// Cache'den veri silmek için kullanılır
        /// </summary>
        /// <param name="key">Cache anahtarı</param>
        /// <returns>İşlem sonucu</returns>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Cache'de bir anahtarın var olup olmadığını kontrol eder
        /// </summary>
        /// <param name="key">Cache anahtarı</param>
        /// <returns>Var ise true, yok ise false</returns>
        Task<bool> ExistsAsync(string key);
    }
} 