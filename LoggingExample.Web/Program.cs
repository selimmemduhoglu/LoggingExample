using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Configurations;
using LoggingExample.Web.Middlewares;
using LoggingExample.Web.Services;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
builder.Host.UseSerilog((context, loggerConfig) =>
{
	// appsettings.json'dan temel ayarları okuyalım
	loggerConfig
		.ReadFrom.Configuration(context.Configuration)
		.Enrich.WithExceptionDetails() // Hata detaylarını ekler
		.Enrich.FromLogContext() // Log context bilgilerini ekler
		.Enrich.WithMachineName() // Makine adını ekler
		.Enrich.WithEnvironmentName() // Ortam bilgisini ekler
		.Enrich.WithProperty("ApplicationName", "LoggingExample") // Uygulama adını ekler
		.Enrich.WithCorrelationId() // Korelasyon ID ekler (request izleme için)
		.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
		.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(context.Configuration["SeriLogConfig:ElasticUri"] ?? "http://elasticsearch:9200"))
		{
			IndexFormat = $"{context.Configuration["SeriLogConfig:ProjectName"]}-{context.Configuration["SeriLogConfig:Environment"]}-logs-{{0:yyyy.MM.dd}}",
			AutoRegisterTemplate = true,
			AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv7,
			ModifyConnectionSettings = conn => conn.BasicAuthentication(
				context.Configuration["SeriLogConfig:ElasticUser"] ?? "elastic123",
				context.Configuration["SeriLogConfig:ElasticPassword"] ?? "elastic1234"
			),
			InlineFields = true,
			MinimumLogEventLevel = LogEventLevel.Information
		})
		.WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://seq:5341",
			restrictedToMinimumLevel: LogEventLevel.Information);
});

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();
builder.Services.AddTransient<RequestResponseLogMiddleware>();
builder.Services.AddSingleton<ILogContextEnricher, LogContextEnricher>();

// HttpClientFactory ekleme (HealthCheck için)
builder.Services.AddHttpClient();

// Health checks
builder.Services.AddHealthChecks()
	.AddCheck<ElasticsearchHealthCheck>("elasticsearch_health_check",
		tags: new[] { "elasticsearch", "ready" });

var app = builder.Build();

// Middleware pipeline
app.UseAllElasticApm(builder.Configuration);

if (app.Environment.IsDevelopment())
{
	app.UseSwaggerServices();
}

// Prometheus metrics middleware
app.UseHttpMetrics(); // HTTP metrics için

// Loglama middleware
app.UseMiddleware<RequestResponseLogMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

// Metrics endpoint
app.MapMetrics(); // Prometheus metrics endpoint

app.MapControllers();
app.UseHealthChecks("/api/health-check");

app.Run();