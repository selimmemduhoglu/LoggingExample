using Microsoft.AspNetCore.Mvc;

namespace LoggingExample.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
	private readonly ILogger<LogController> _logger;

	public LogController(ILogger<LogController> logger)
	{
		_logger = logger;
	}

	[HttpGet("debug")]
	public IActionResult LogDebug()
	{
		_logger.LogDebug("Bu bir DEBUG log mesajıdır");
		return Ok("Debug log gönderildi");
	}

	[HttpGet("info")]
	public IActionResult LogInfo()
	{
		_logger.LogInformation("Bu bir INFO log mesajıdır");
		return Ok("Info log gönderildi");
	}

	[HttpGet("warning")]
	public IActionResult LogWarning()
	{
		_logger.LogWarning("Bu bir WARNING log mesajıdır");
		return Ok("Warning log gönderildi");
	}

	[HttpGet("error")]
	public IActionResult LogError()
	{
		try
		{
			throw new Exception("Test hatası fırlatıldı");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Bu bir ERROR log mesajıdır");
			return StatusCode(500, "Error log gönderildi");
		}
	}

	[HttpGet("structured")]
	public IActionResult LogStructured()
	{
		// Structured logging örneği
		_logger.LogInformation("Kullanıcı {UserId} tarafından {ActionName} işlemi gerçekleştirildi", 123, "Giriş");

		return Ok("Structured log gönderildi");
	}

	[HttpGet("performance")]
	public IActionResult LogPerformance()
	{
		using (_logger.BeginScope("PerformanceTest"))
		{
			_logger.LogInformation("Performans testi başladı");

			// İşlem simülasyonu
			System.Threading.Thread.Sleep(2000);

			_logger.LogInformation("Performans testi tamamlandı");
		}

		return Ok("Performans log gönderildi");
	}
}


