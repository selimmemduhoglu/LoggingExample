using System.Net;

namespace LoggingExample.Web.Models.Exceptions
{
    /// <summary>
    /// Temel API Exception sınıfı
    /// </summary>
    public abstract class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public List<string> ErrorDetails { get; } = new List<string>();

        protected ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
        
        protected ApiException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public virtual void AddErrorDetail(string detail)
        {
            ErrorDetails.Add(detail);
        }
    }

    /// <summary>
    /// Bulunamadı hatası (404)
    /// </summary>
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message = "Kayıt bulunamadı") 
            : base(message, HttpStatusCode.NotFound)
        {
        }

        public NotFoundException(string entityName, object id) 
            : base($"{entityName} kaydı bulunamadı: {id}", HttpStatusCode.NotFound)
        {
            AddErrorDetail($"{entityName} (ID: {id}) sistemde kayıtlı değil");
        }
    }

    /// <summary>
    /// Geçersiz istek hatası (400)
    /// </summary>
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message = "Geçersiz istek") 
            : base(message, HttpStatusCode.BadRequest)
        {
        }

        public BadRequestException(string message, List<string> errors) 
            : base(message, HttpStatusCode.BadRequest)
        {
            foreach (var error in errors)
            {
                AddErrorDetail(error);
            }
        }
    }

    /// <summary>
    /// İzin hatası (403)
    /// </summary>
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Bu işlem için yetkiniz bulunmamaktadır") 
            : base(message, HttpStatusCode.Forbidden)
        {
        }
    }

    /// <summary>
    /// Yetkilendirme hatası (401) 
    /// </summary>
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "Lütfen giriş yapınız") 
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>
    /// İş mantığı hatası (409)
    /// </summary>
    public class BusinessException : ApiException
    {
        public BusinessException(string message) 
            : base(message, HttpStatusCode.Conflict)
        {
        }

        public BusinessException(string message, List<string> errors) 
            : base(message, HttpStatusCode.Conflict)
        {
            foreach (var error in errors)
            {
                AddErrorDetail(error);
            }
        }
    }

    /// <summary>
    /// Veri tutarlılık hatası (422)
    /// </summary>
    public class ValidationException : ApiException
    {
        public ValidationException(string message = "Doğrulama hatası") 
            : base(message, HttpStatusCode.UnprocessableEntity)
        {
        }

        public ValidationException(string message, List<string> errors) 
            : base(message, HttpStatusCode.UnprocessableEntity)
        {
            foreach (var error in errors)
            {
                AddErrorDetail(error);
            }
        }
    }

    /// <summary>
    /// Dış servis hatası (502)
    /// </summary>
    public class ExternalServiceException : ApiException
    {
        public string ServiceName { get; }

        public ExternalServiceException(string serviceName, string message)
            : base($"{serviceName} servisi hatası: {message}", HttpStatusCode.BadGateway)
        {
            ServiceName = serviceName;
            AddErrorDetail($"{serviceName} servisinde hata oluştu");
        }

        public ExternalServiceException(string serviceName, string message, Exception innerException)
            : base($"{serviceName} servisi hatası: {message}", innerException, HttpStatusCode.BadGateway)
        {
            ServiceName = serviceName;
            AddErrorDetail($"{serviceName} servisinde hata oluştu: {innerException.Message}");
        }
    }
} 