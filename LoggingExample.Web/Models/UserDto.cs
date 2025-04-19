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
        public string Username { get; set; }
        
        /// <summary>
        /// E-posta adresi
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Şifre
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Yaş
        /// </summary>
        public int? Age { get; set; }
        
        /// <summary>
        /// Doğum tarihi
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
} 