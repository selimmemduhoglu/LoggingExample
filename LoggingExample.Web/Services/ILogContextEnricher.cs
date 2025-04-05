using Serilog.Context;
using System.Security.Claims;

namespace LoggingExample.Web.Services
{
	public interface ILogContextEnricher
	{
		IDisposable EnrichFromRequest(HttpContext context, string correlationId);
	}

	public class LogContextEnricher : ILogContextEnricher
	{
		public IDisposable EnrichFromRequest(HttpContext context, string correlationId)
		{
			var disposables = new List<IDisposable>
	        {
                // Temel bilgiler
                LogContext.PushProperty("CorrelationId", correlationId),
	        	LogContext.PushProperty("RequestId", context.TraceIdentifier),
	        	LogContext.PushProperty("RequestMethod", context.Request.Method),
	        	LogContext.PushProperty("RequestPath", context.Request.Path),
	        	LogContext.PushProperty("RequestProtocol", context.Request.Protocol),
	        	LogContext.PushProperty("RequestScheme", context.Request.Scheme),
                
                // Kullanıcı bilgileri
                LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()),
	        	LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress),
	        	LogContext.PushProperty("Host", context.Request.Host.ToString()),
                
                // İstek detayları
                LogContext.PushProperty("QueryString", context.Request.QueryString.ToString()),
	        	LogContext.PushProperty("ContentType", context.Request.ContentType),
	        	LogContext.PushProperty("ContentLength", context.Request.ContentLength),
                
                // ASP.NET Core bilgileri
                LogContext.PushProperty("ConnectionId", context.Connection.Id),
	        	LogContext.PushProperty("RouteData", string.Join(", ", context.GetRouteData()?.Values?.Select(v => $"{v.Key}={v.Value}") ?? Array.Empty<string>()))
	        };

			// Belirli önemli HTTP header'ları ekle
			var importantHeaders = new[] { "Referer", "Origin", "X-Forwarded-For", "X-Real-IP", "Accept-Language" };
			foreach (var header in importantHeaders)
			{
				if (context.Request.Headers.TryGetValue(header, out var value))
				{
					disposables.Add(LogContext.PushProperty($"Header_{header}", value.ToString()));
				}
			}

			// Kullanıcı kimlik doğrulama bilgileri
			if (context.User?.Identity?.IsAuthenticated == true)
			{
				disposables.Add(LogContext.PushProperty("UserId", context.User.FindFirst("sub")?.Value));
				disposables.Add(LogContext.PushProperty("UserName", context.User.Identity.Name));

				// İsteğe bağlı: Kullanıcının rol ve claim'lerini de logla
				var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
				disposables.Add(LogContext.PushProperty("UserRoles", string.Join(", ", roles)));
			}

			return new CompositeDisposable(disposables.ToArray());
		}
	}

	// Birden fazla IDisposable'ı yönetmek için yardımcı sınıf
	public class CompositeDisposable : IDisposable
	{
		private readonly IDisposable[] _disposables;

		public CompositeDisposable(params IDisposable[] disposables)
		{
			_disposables = disposables.Where(d => d != null).ToArray();
		}

		public void Dispose()
		{
			foreach (var disposable in _disposables)
			{
				disposable?.Dispose();
			}
		}
	}
}