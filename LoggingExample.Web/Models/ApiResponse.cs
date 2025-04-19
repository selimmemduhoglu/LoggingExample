using System.Net;

namespace LoggingExample.Web.Models
{
    /// <summary>
    /// Tüm API yanıtları için standart response yapısı
    /// </summary>
    /// <typeparam name="T">Yanıt veri tipi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// İşlem başarı durumu
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP durum kodu
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Mesaj
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Dönen veri
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Varsa hata detayları
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Korelasyon ID'si (istek takibi için)
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Başarılı yanıt oluşturur
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "İşlem başarılı", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = (int)statusCode,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Hata yanıtı oluşturur
        /// </summary>
        public static ApiResponse<T> CreateFailure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = (int)statusCode,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
} 