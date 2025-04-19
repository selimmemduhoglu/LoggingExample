using System.Diagnostics;
using System.Net;
using System.Text.Json;
using LoggingExample.Web.Models;
using LoggingExample.Web.Models.Exceptions;
using LoggingExample.Web.Services;

namespace LoggingExample.Web.Middleware
{
    /// <summary>
    /// Global Exception Handler Middleware
    /// </summary>
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;
        private readonly ILogContextEnricher _logEnricher;
        
        public GlobalExceptionHandlerMiddleware(
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment environment,
            ILogContextEnricher logEnricher)
        {
            _logger = logger;
            _environment = environment;
            _logEnricher = logEnricher;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // İşlem başlangıç zamanı (logging için)
            var startTime = Stopwatch.GetTimestamp();
            
            // Korelasyon ID
            string correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            // Activity için TraceId alımı
            string traceId = Activity.Current?.TraceId.ToString() ?? "";
            
            // Exception tipi ve durum kodu belirleme
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
            List<string> errorDetails = new List<string>();
            
            // Log Context'e değerler ekleyelim
            using (_logEnricher.EnrichFromRequest(context, correlationId, traceId))
            {
                // Kendi özel Exception'larımızı işle
                if (exception is ApiException apiException)
                {
                    statusCode = apiException.StatusCode;
                    message = apiException.Message;
                    errorDetails = apiException.ErrorDetails;
                    
                    // Business ve Validation hataları için Warning seviyesinde logla
                    if (exception is BusinessException || exception is ValidationException)
                    {
                        _logger.LogWarning(
                            exception,
                            "İş Kuralı/Validasyon Hatası: {ExceptionType} - {Message}",
                            exception.GetType().Name,
                            exception.Message);
                    }
                    else
                    {
                        _logger.LogError(
                            exception,
                            "API Hatası: {ExceptionType} - {StatusCode} - {Message}",
                            exception.GetType().Name,
                            (int)statusCode,
                            exception.Message);
                    }
                }
                else
                {
                    // Sistem hatalarını Error seviyesinde logla
                    _logger.LogError(
                        exception,
                        "Sistem Hatası: {ExceptionType} - {Message}",
                        exception.GetType().Name,
                        exception.Message);
                    
                    // Sadece geliştirme ortamında gerçek hata mesajlarını göster
                    if (_environment.IsDevelopment())
                    {
                        message = exception.Message;
                        errorDetails.Add(exception.StackTrace ?? "");
                    }
                }
                
                // ApiResponse oluştur
                var response = ApiResponse<object>.CreateFailure(message, statusCode, errorDetails);
                response.CorrelationId = correlationId;
                
                // Response'u ayarla
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;
                
                // İşlem süresini hesapla ve response header'a ekle
                var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());
                context.Response.Headers["X-Error-Time"] = elapsedMs.ToString();
                
                // Yanıtı gönder
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = _environment.IsDevelopment()
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        }
        
        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
} 