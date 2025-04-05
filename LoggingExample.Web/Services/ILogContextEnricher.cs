using Serilog.Context;

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
			// Değerleri zincirleme şekilde ekleyip tek bir IDisposable döndürüyoruz
			var disposable1 = LogContext.PushProperty("CorrelationId", correlationId);
			var disposable2 = LogContext.PushProperty("RequestId", context.TraceIdentifier);
			var disposable3 = LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString());
			var disposable4 = LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress);

			// Kullanıcı kimlik bilgileri varsa ekle
			if (context.User?.Identity?.IsAuthenticated == true)
			{
				var disposable5 = LogContext.PushProperty("UserId", context.User.FindFirst("sub")?.Value);
				var disposable6 = LogContext.PushProperty("UserName", context.User.Identity.Name);

				return new CompositeDisposable(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
			}

			return new CompositeDisposable(disposable1, disposable2, disposable3, disposable4);
		}
	}

	// Birden fazla IDisposable'ı yönetmek için yardımcı sınıf
	public class CompositeDisposable : IDisposable
	{
		private readonly IDisposable[] _disposables;

		public CompositeDisposable(params IDisposable[] disposables)
		{
			_disposables = disposables;
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