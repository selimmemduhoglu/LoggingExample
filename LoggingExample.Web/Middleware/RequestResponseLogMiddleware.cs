using System.Diagnostics;
using LoggingExample.Web.Services;
using Microsoft.IO;

namespace LoggingExample.Web.Middlewares
{
	public class RequestResponseLogMiddleware : IMiddleware
	{
		private readonly ILogger<RequestResponseLogMiddleware> _logger;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
		private readonly ILogContextEnricher _logEnricher;

		public RequestResponseLogMiddleware(
			ILoggerFactory loggerFactory,
			ILogContextEnricher logEnricher)
		{
			_logger = loggerFactory.CreateLogger<RequestResponseLogMiddleware>();
			_recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
			_logEnricher = logEnricher;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			// İstek başlangıç zamanı
			var startTime = Stopwatch.GetTimestamp();

			// Korelasyon ID (request izleme için)
			var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
			context.Response.Headers["X-Correlation-ID"] = correlationId;

			// Log Context'e değerler ekleyelim
			using (_logEnricher.EnrichFromRequest(context, correlationId))
			{
				// İstek detaylarını logla
				_logger.LogInformation("HTTP {RequestMethod} {RequestPath} başladı",
					context.Request.Method,
					context.Request.Path);

				// İstek Body'sini logla (gerekirse)
				string requestBody = await GetRequestBodyAsync(context.Request);

				try
				{
					// Yanıtı intercept etmek için MemoryStream hazırla
					var originalBodyStream = context.Response.Body;
					await using var responseBodyStream = _recyclableMemoryStreamManager.GetStream();
					context.Response.Body = responseBodyStream;

					// Pipeline'da bir sonraki middleware'i çağır
					await next(context);

					// Yanıt Body'sini logla
					context.Response.Body.Position = 0;
					var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

					// İşlem süresini hesapla
					var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());

					// Yanıt detaylarını logla
					_logger.LogInformation(
						"HTTP {RequestMethod} {RequestPath} tamamlandı - {StatusCode} in {ElapsedMilliseconds}ms",
						context.Request.Method,
						context.Request.Path,
						context.Response.StatusCode,
						elapsedMs);

					// Geniş detaylı loglama (ihtiyaca göre)
					if (context.Response.StatusCode >= 400)
					{
						// Sadece hata durumlarında request/response body'leri logla
						_logger.LogWarning(
							"HTTP {RequestMethod} {RequestPath} sorunu - İstek: {RequestBody}, Yanıt: {ResponseBody}",
							context.Request.Method,
							context.Request.Path,
							requestBody,
							responseBody);
					}

					// Yanıtı orijinal stream'e kopyala
					context.Response.Body.Position = 0;
					await responseBodyStream.CopyToAsync(originalBodyStream);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "HTTP {RequestMethod} {RequestPath} işlenirken hata oluştu",
						context.Request.Method,
						context.Request.Path);
					throw;
				}
			}
		}

		private static double GetElapsedMilliseconds(long start, long stop)
		{
			return (stop - start) * 1000 / (double)Stopwatch.Frequency;
		}

		private async Task<string> GetRequestBodyAsync(HttpRequest request)
		{
			// POST, PUT gibi body içeren istekler için
			if (request.ContentLength > 0)
			{
				request.EnableBuffering();

				using var streamReader = new StreamReader(
					request.Body,
					encoding: System.Text.Encoding.UTF8,
					detectEncodingFromByteOrderMarks: false,
					leaveOpen: true);

				var requestBody = await streamReader.ReadToEndAsync();
				request.Body.Position = 0;
				return requestBody;
			}

			return string.Empty;
		}
	}
}