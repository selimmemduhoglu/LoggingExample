using LoggingExample.Web.Models.Kafka;
using LoggingExample.Web.Services;
using LoggingExample.Web.Services.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoggingExample.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class KafkaController : ControllerBase
	{
		private readonly KafkaProducerService _kafkaProducerService;

		public KafkaController(KafkaProducerService kafkaProducerService)
		{
			_kafkaProducerService = kafkaProducerService;
		}

		[HttpPost("send")]
		public async Task<IActionResult> SendMessage([FromBody] KafkaMessageModel model)
		{
			string json = JsonSerializer.Serialize(model);
			await _kafkaProducerService.ProduceAsync(json);
			return Ok("Message sent to Kafka");
		}

	}
}
