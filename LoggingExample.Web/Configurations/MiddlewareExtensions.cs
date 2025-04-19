using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Middleware;
using LoggingExample.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace LoggingExample.Web.Configurations
{
    /// <summary>
    /// Middleware yapılandırmaları için extension metotlar
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Tüm middleware'leri yapılandırır
        /// </summary>
        public static WebApplication ConfigureAllMiddlewares(this WebApplication app, IConfiguration configuration)
        {
            return app
                .ConfigureElasticApm(configuration)
                .ConfigureSwaggerUI()
                .ConfigureExceptionHandling()
                .ConfigureMetrics()
                .ConfigureLogging()
                .ConfigureEndpoints();
        }

        /// <summary>
        /// Elastic APM middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureElasticApm(this WebApplication app, IConfiguration configuration)
        {
            app.UseAllElasticApm(configuration);
            return app;
        }

        /// <summary>
        /// Swagger UI middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureSwaggerUI(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerServices();
            }
            return app;
        }

        /// <summary>
        /// Exception handling middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureExceptionHandling(this WebApplication app)
        {
            // Global exception handler middleware - Request/Response log middleware'den önce ekle
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            return app;
        }

        /// <summary>
        /// Prometheus metrics middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureMetrics(this WebApplication app)
        {
            // Prometheus metrics middleware
            app.UseHttpMetrics(); // HTTP metrics için
            return app;
        }

        /// <summary>
        /// Loglama middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureLogging(this WebApplication app)
        {
            // Loglama middleware
            app.UseMiddleware<RequestResponseLogMiddleware>();
            return app;
        }

        /// <summary>
        /// Endpoint middleware yapılandırması
        /// </summary>
        public static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            app.UseAuthorization();

            // Metrics endpoint
            app.MapMetrics(); // Prometheus metrics endpoint

            app.MapControllers();
            app.UseHealthChecks("/api/health-check");
            
            return app;
        }
    }
} 