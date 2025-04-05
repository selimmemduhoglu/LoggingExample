using Confluent.Kafka;

namespace LoggingExample.Web.Services.Kafka
{
	public class KafkaProducerService
	{
		private readonly IProducer<Null, string> _producer;
		private readonly string _topic;

		public KafkaProducerService(IConfiguration configuration)
		{
			var config = new ProducerConfig
			{
				BootstrapServers = configuration["Kafka:BootstrapServers"]
			};

			_producer = new ProducerBuilder<Null, string>(config).Build();
			_topic = configuration["Kafka:Topic"];
		}

		public async Task ProduceAsync(string message)
		{
			try
			{
				var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
				Console.WriteLine($"Message '{result.Value}' sent to '{result.TopicPartitionOffset}'");
			}
			catch (ProduceException<Null, string> e)
			{
				Console.WriteLine($"Delivery failed: {e.Error.Reason}");
			}
		}
	}
}
