using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Configurations;
using LoggingExample.Web.Extensions;
using LoggingExample.Web.Middlewares;
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

builder.Services.AddTransient<RequestResponseLogMiddleware>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAllElasticApm(builder.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerServices();
}
app.UseHttpsRedirection();
app.UseMiddleware<RequestResponseLogMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.UseHealthChecks("/api/health-check");

app.Run();