namespace LoggingExample.Web.Models.OptionModels
{
    /// <summary>
    /// Redis yapılandırma ayarları
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Redis bağlantı dizesi
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        
        /// <summary>
        /// Redis instance adı (ön ek)
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Önbellek girdileri için varsayılan mutlak son kullanma süresi (dakika)
        /// </summary>
        public int AbsoluteExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Önbellek girdileri için varsayılan kayar son kullanma süresi (dakika)
        /// </summary>
        public int SlidingExpirationMinutes { get; set; } = 20;
    }
} 