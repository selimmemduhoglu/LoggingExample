using Microsoft.AspNetCore.Mvc;
using LoggingExample.Web.Models;
using LoggingExample.Web.Models.Exceptions;

namespace LoggingExample.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ExceptionTestController : ControllerBase
	{
		private readonly ILogger<ExceptionTestController> _logger;

		public ExceptionTestController(ILogger<ExceptionTestController> logger)
		{
			_logger = logger;
		}

		[HttpGet("success")]
		public IActionResult GetSuccess()
		{
			var data = new { Message = "Başarılı cevap", Date = DateTime.Now };
			var response = ApiResponse<object>.CreateSuccess(data, "İşlem başarılı");

			// Korelasyon ID'sini yeni oluşturursak response'a ekleyelim
			if (HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
			{
				response.CorrelationId = correlationId;
			}

			return Ok(response);
		}

		[HttpGet("not-found")]
		public IActionResult GetNotFound()
		{
			// 404 hatası fırlat
			throw new NotFoundException("Kullanıcı", 123);
		}

		[HttpGet("bad-request")]
		public IActionResult GetBadRequest()
		{
			// 400 hatası fırlat
			var errors = new List<string>
			{
				"Email formatı geçersiz",
				"Şifre en az 8 karakter olmalıdır"
			};
			throw new BadRequestException("Form verilerinde hatalar var", errors);
		}

		[HttpGet("unauthorized")]
		public IActionResult GetUnauthorized()
		{
			// 401 hatası fırlat
			throw new UnauthorizedException("Bu işlem için giriş yapmalısınız");
		}

		[HttpGet("forbidden")]
		public IActionResult GetForbidden()
		{
			// 403 hatası fırlat
			throw new ForbiddenException("Bu işlem için yetkiniz bulunmamaktadır");
		}

		[HttpGet("validation")]
		public IActionResult GetValidationError()
		{
			// 422 hatası fırlat
			var errors = new List<string>
			{
				"Ad alanı zorunludur",
				"Tarih geçmiş bir tarih olamaz",
				"Miktar 0'dan büyük olmalıdır"
			};
			throw new ValidationException("Veri doğrulama hatası", errors);
		}

		[HttpGet("business")]
		public IActionResult GetBusinessError()
		{
			// 409 hatası fırlat
			throw new BusinessException("Bu ürün stokta bulunmamaktadır");
		}

		[HttpGet("external-service")]
		public IActionResult GetExternalServiceError()
		{
			// 502 hatası fırlat
			throw new ExternalServiceException(
				"PaymentAPI",
				"Ödeme servisi şu anda çalışmıyor",
				new Exception("Connection timed out"));
		}

		[HttpGet("server-error")]
		public IActionResult GetServerError()
		{
			// 500 hatası fırlat (standart exception)
			throw new Exception("Sistemde beklenmeyen bir hata oluştu!");
		}

		[HttpGet("divide-by-zero")]
		public IActionResult GetDivideByZero()
		{
			// Sıfıra bölme hatası
			_logger.LogInformation("Sıfıra bölme işlemi başlıyor");

			int divizor = 0;
			var result = 100 / divizor;

			return Ok(result);
		}

		[HttpGet("null-reference")]
		public IActionResult GetNullReference()
		{
			// Null reference hatası
			string nullString = null;
			return Ok(nullString.Length);
		}
	}
}