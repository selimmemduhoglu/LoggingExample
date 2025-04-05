using Confluent.Kafka;

namespace LoggingExample.Web.Services.Kafka
{
	public class KafkaConsumerService : BackgroundService
	{
		private readonly IConsumer<Null, string> _consumer;
		private readonly string _topic;

		public KafkaConsumerService(IConfiguration configuration)
		{
			var config = new ConsumerConfig
			{
				BootstrapServers = configuration["Kafka:BootstrapServers"],
				GroupId = configuration["Kafka:GroupId"],
				AutoOffsetReset = AutoOffsetReset.Earliest
			};

			_consumer = new ConsumerBuilder<Null, string>(config).Build();
			_topic = configuration["Kafka:Topic"];
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_consumer.Subscribe(_topic);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var consumeResult = _consumer.Consume(stoppingToken);
					Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
				}
				catch (ConsumeException e)
				{
					Console.WriteLine($"Consume error: {e.Error.Reason}");
				}

				await Task.Delay(1000, stoppingToken);
			}
		}

		public override void Dispose()
		{
			_consumer.Close();
			_consumer.Dispose();
			base.Dispose();
		}
	}
}
