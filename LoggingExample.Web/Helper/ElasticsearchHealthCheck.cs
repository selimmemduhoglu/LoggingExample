using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LoggingExample.Web.Helper
{
    public class ElasticsearchHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly string _elasticsearchUrl;

        public ElasticsearchHealthCheck(string elasticsearchUrl)
        {
            _httpClient = new HttpClient();
            _elasticsearchUrl = elasticsearchUrl;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_elasticsearchUrl}/_cluster/health", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                // "green" veya "yellow" durumunu kabul et
                if (content.Contains("green") || content.Contains("yellow"))
                {
                    return HealthCheckResult.Healthy("Elasticsearch is running.");
                }

                return HealthCheckResult.Degraded("Elasticsearch cluster status is red.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Elasticsearch connection failed.", ex);
            }
        }
    }
}
