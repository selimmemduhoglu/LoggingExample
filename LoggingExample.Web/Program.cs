using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Configurations;
using LoggingExample.Web.Middlewares;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//// Serilog konfig�rasyonu
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)); // appsetting'te ki konfig�rasyonu okusun diye var.

//builder.Configuration.RegisterLogger();
//builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerServices();

builder.Services.AddTransient<RequestResponseLogMiddleware>(); // Middleware'i ekleyelim
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAllElasticApm(builder.Configuration);    // Elastic APM'i ekleyelim

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerServices();
}
app.UseHttpsRedirection();
app.UseMiddleware<RequestResponseLogMiddleware>();

app.UseAuthorization();



app.MapControllers(); // Routing'i ekle
app.UseHealthChecks("/api/health-check"); // Health check endpoint'i ekle

app.Run(); 