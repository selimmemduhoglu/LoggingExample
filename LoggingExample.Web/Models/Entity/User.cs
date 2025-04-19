using System.ComponentModel.DataAnnotations;

namespace LoggingExample.Web.Models.Entity
{
    /// <summary>
    /// Kullanıcı entity sınıfı
    /// </summary>
    public class User
    {
        /// <summary>
        /// Kullanıcı ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcı adı
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// E-posta adresi
        /// </summary>
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Şifre Hash'i
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Kullanıcı rolü
        /// </summary>
        [MaxLength(20)]
        public string Role { get; set; } = "User";
        
        /// <summary>
        /// Hesap aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Hesap oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Son güncelleme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
} 