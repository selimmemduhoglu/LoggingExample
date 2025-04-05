using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Configurations;
using LoggingExample.Web.Middlewares;
using LoggingExample.Web.Services;
using Prometheus;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
// Serilog konfigürasyonu
builder.Host.UseSerilog((context, loggerConfig) =>
{
	loggerConfig
		.ReadFrom.Configuration(context.Configuration)
		.Enrich.WithExceptionDetails()
		.Enrich.FromLogContext()
		.Enrich.WithMachineName()
		.Enrich.WithEnvironmentName()
		.Enrich.WithProperty("ApplicationName", "LoggingExample")
		.Enrich.WithCorrelationId()
		.Enrich.WithSpan() // OpenTelemetry span bilgilerini ekler
		// Elastic.Net istemci loglarını filtrele
		.Filter.ByExcluding(c => c.Properties.ContainsKey("SourceContext") &&
						   (c.Properties["SourceContext"].ToString().Contains("HttpConnectionDiagnosticsListener") ||
							c.Properties["SourceContext"].ToString().Contains("RequestPipelineDiagnosticsListener")))
		.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
		.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(context.Configuration["SeriLogConfig:ElasticUri"] ?? "http://elasticsearch:9200"))
		{
			IndexFormat = $"{context.Configuration["SeriLogConfig:ProjectName"]}-{context.Configuration["SeriLogConfig:Environment"]}-logs-{{0:yyyy.MM.dd}}",
			AutoRegisterTemplate = true,
			AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv7,
			ModifyConnectionSettings = conn => conn.BasicAuthentication(
				context.Configuration["SeriLogConfig:ElasticUser"] ?? "elastic",
				context.Configuration["SeriLogConfig:ElasticPassword"] ?? "elastic1234"
			)
		})
		.WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://seq:5341",
			restrictedToMinimumLevel: LogEventLevel.Information);
});

// Add OpenTelemetry services // For jaeger
builder.Services.AddOpenTelemetry() // OpenTelemetry izleme hizmetlerini ekliyoruz.
	.ConfigureResource(resource => resource
		.AddService("LoggingExample.Web"))  // Bu, izleme kaynaklarını yapılandırıyor ve servis adı olarak "LoggingExample.Web" belirliyoruz.
	.WithTracing(tracerProviderBuilder =>  // İzleme (tracing) yapılandırmasını başlatıyoruz.
	{
		tracerProviderBuilder
			.AddAspNetCoreInstrumentation(options =>  // ASP.NET Core uygulamasında izleme yapabilmek için gerekli yapılandırmayı ekliyoruz.
			{
				options.RecordException = true;  // Eğer bir hata oluşursa, bu hatayı kaydediyoruz.
				options.EnrichWithHttpRequest = (activity, request) => // HTTP istekleri ile ilgili ekstra bilgiler ekliyoruz.
				{
					activity.SetTag("http.request.header.x-correlation-id",
						request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString());
					// "X-Correlation-ID" başlığına göre bir "correlation ID" set ediyoruz. Eğer başlık yoksa, yeni bir GUID oluşturuyoruz.
				};
			})
			.AddHttpClientInstrumentation(options => // HTTP istemcisi (HttpClient) için izleme yapılandırması ekliyoruz.
			{
				options.RecordException = true; // HTTP isteklerinde hata meydana gelirse kaydediyoruz.
				options.FilterHttpRequestMessage = (request) => // İstekler için filtreleme işlemi.
				{
					//  // Elasticsearch ve Seq gibi altyapı isteklerini filtreliyoruz.
					if (request.RequestUri?.Host.Contains("elasticsearch") == true ||
						request.RequestUri?.Host.Contains("seq") == true)
						return false; // Elasticsearch ve Seq istekleri için izleme yapılmasın.
					return true; // Didğer tüm istekler için izleme yapılmasını sağlıyor.
				};
			})
			.AddOtlpExporter(options => // OpenTelemetry için OTLP (OpenTelemetry Protocol) exporter ekliyoruz.
			{
				options.Endpoint = new Uri( 
					builder.Environment.IsDevelopment() // Uygulama geliştirme ortamında mı diye kontrol ediyoruz.
						? "http://localhost:4317" // Geliştirme ortamında Jaeger'e istek gönderiyoruz.
						: "http://jaeger:4317" // Üretim ortamında ise Jaeger konteynerine bağlanıyoruz.
				);
			});
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

// HTTPS yönlendirme tamamen kaldırıldı
// app.UseHttpsRedirection();

app.UseAuthorization();

// Metrics endpoint
app.MapMetrics(); // Prometheus metrics endpoint

app.MapControllers();
app.UseHealthChecks("/api/health-check");

app.Run();