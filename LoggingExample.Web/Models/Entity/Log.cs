using System.ComponentModel.DataAnnotations;

namespace LoggingExample.Web.Models.Entity
{
    /// <summary>
    /// Uygulama log entity sınıfı
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Log ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Log seviyesi (Info, Warning, Error, vs.)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Level { get; set; } = string.Empty;
        
        /// <summary>
        /// Log mesajı
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// İlgili modül veya sınıf
        /// </summary>
        [MaxLength(100)]
        public string? Source { get; set; }
        
        /// <summary>
        /// İstek kimliği
        /// </summary>
        [MaxLength(50)]
        public string? RequestId { get; set; }
        
        /// <summary>
        /// Korelasyon kimliği
        /// </summary>
        [MaxLength(50)]
        public string? CorrelationId { get; set; }
        
        /// <summary>
        /// Kullanıcı kimliği
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// IP adresi
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// Tarayıcı veya uygulama bilgisi
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// Log tarihi
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Exception detayları
        /// </summary>
        public string? Exception { get; set; }
        
        /// <summary>
        /// Ek veriler (JSON olarak saklanabilir)
        /// </summary>
        public string? AdditionalData { get; set; }
    }
} 