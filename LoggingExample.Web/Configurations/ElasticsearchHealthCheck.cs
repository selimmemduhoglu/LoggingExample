using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace LoggingExample.Web.Configurations
{
	public class ElasticsearchHealthCheck : IHealthCheck
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _elasticsearchUrl;
		private readonly string _username;
		private readonly string _password;
		private readonly ILogger<ElasticsearchHealthCheck> _logger;

		public ElasticsearchHealthCheck(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<ElasticsearchHealthCheck> logger)
		{
			_httpClientFactory = httpClientFactory;
			_elasticsearchUrl = configuration["SeriLogConfig:ElasticUri"] ?? "http://elasticsearch:9200";
			_username = configuration["ElasticCredentials:Username"] ?? configuration["SeriLogConfig:ElasticUser"] ?? "elastic";
			_password = configuration["ElasticCredentials:Password"] ?? configuration["SeriLogConfig:ElasticPassword"] ?? "changeme";
			_logger = logger;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			try
			{
				var httpClient = _httpClientFactory.CreateClient("ElasticsearchHealthCheck");

				// Basic authentication ekle
				var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_username}:{_password}");
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
					"Basic", Convert.ToBase64String(byteArray));

				var response = await httpClient.GetAsync($"{_elasticsearchUrl}/_cluster/health", cancellationToken);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync(cancellationToken);

				// JSON olarak parse et
				var healthData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

				if (healthData != null && healthData.TryGetValue("status", out var statusElement))
				{
					var status = statusElement.GetString();

					switch (status)
					{
						case "green":
							return HealthCheckResult.Healthy("Elasticsearch is in optimal condition.");
						case "yellow":
							return HealthCheckResult.Degraded("Elasticsearch is in a warning state but functional.");
						case "red":
							return HealthCheckResult.Unhealthy("Elasticsearch is in critical condition.");
						default:
							return HealthCheckResult.Degraded($"Elasticsearch is in unknown state: {status}");
					}
				}

				return HealthCheckResult.Degraded("Could not determine Elasticsearch health status.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Elasticsearch health check failed");
				return HealthCheckResult.Unhealthy("Elasticsearch connection failed.", ex);
			}
		}
	}
}