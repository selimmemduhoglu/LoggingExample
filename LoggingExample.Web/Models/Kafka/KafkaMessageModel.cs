namespace LoggingExample.Web.Models.Kafka
{
	public class KafkaMessageModel
	{
		public string Id { get; set; }
		public string Message { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
