using System.ComponentModel.DataAnnotations;

namespace LoggingExample.Web.Models
{
    /// <summary>
    /// Kullanıcı veri transfer nesnesi
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Kullanıcı ID
        /// </summary>
        public int? Id { get; set; }
        
        /// <summary>
        /// Kullanıcı adı
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
        public string Username { get; set; }
        
        /// <summary>
        /// E-posta adresi
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }
        
        /// <summary>
        /// Şifre
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir")]
        public string Password { get; set; }
        
        /// <summary>
        /// Yaş
        /// </summary>
        [Range(18, 120, ErrorMessage = "Yaş 18-120 arasında olmalıdır")]
        public int? Age { get; set; }
        
        /// <summary>
        /// Doğum tarihi
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }
} 