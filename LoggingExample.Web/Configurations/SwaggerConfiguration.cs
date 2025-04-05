using Microsoft.OpenApi.Models;

namespace LoggingExample.Web.Configurations;

public static class SwaggerConfiguration
{
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LoggingExample API",
                Version = "v1",
                Description = "API for LoggingExample Web Application",
            });

        });
    }

    public static void UseSwaggerServices(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoggingExample API V1");
        });
    }
}
