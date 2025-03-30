using Prometheus;

namespace LoggingExample.Web.Middlewares;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path.Value;
        string method = context.Request.Method;

        Counter counter = Metrics.CreateCounter("api_requests_total", "HTTP Requests Total",
            new CounterConfiguration
            {
                LabelNames = new[] { "path", "method", "status" }
            });

        string statusCode = "unknown";
        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode.ToString();
        }
        finally
        {
            counter.Labels(path, method, statusCode).Inc();
        }
    }
}