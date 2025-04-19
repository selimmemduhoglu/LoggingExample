using System.ComponentModel.DataAnnotations;

namespace LoggingExample.Web.Models.Entity
{
    /// <summary>
    /// Önbelleğe alınan istekler için entity sınıfı
    /// </summary>
    public class CachedRequest
    {
        /// <summary>
        /// Önbellek kaydı ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Önbellek anahtarı
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string CacheKey { get; set; } = string.Empty;
        
        /// <summary>
        /// İstek URL'i
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string RequestUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// HTTP metodu (GET, POST, vb.)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string HttpMethod { get; set; } = "GET";
        
        /// <summary>
        /// Önbelleğe eklenme tarihi
        /// </summary>
        public DateTime CachedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Son erişim tarihi
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Önbellek veri boyutu (byte)
        /// </summary>
        public long DataSize { get; set; }
        
        /// <summary>
        /// Önbellek hit sayısı
        /// </summary>
        public int HitCount { get; set; } = 0;
        
        /// <summary>
        /// Önbellek son kullanma tarihi
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// İsteği yapan kullanıcı ID (eğer varsa)
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// Korelasyon kimliği
        /// </summary>
        [MaxLength(50)]
        public string? CorrelationId { get; set; }
    }
} 