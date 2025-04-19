using FluentValidation;
using FluentValidation.AspNetCore;
using LoggingExample.Web.Data;
using LoggingExample.Web.Filters;
using LoggingExample.Web.Middleware;
using LoggingExample.Web.Middlewares;
using LoggingExample.Web.Models.OptionModels;
using LoggingExample.Web.Repositories;
using LoggingExample.Web.Services;
using LoggingExample.Web.Services.Cache;
using LoggingExample.Web.Services.Kafka;
using LoggingExample.Web.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using LoggingExample.Web.Data.EntityConfigurations;
using LoggingExample.Web.Repositories.UnitOfWork;
namespace LoggingExample.Web.Configurations
{
	/// <summary>
	/// Servis yapılandırmaları için extension metotlar
	/// </summary>
	public static class ServiceExtensions
	{
		/// <summary>
		/// Tüm uygulama servislerini yapılandırır ve ekler
		/// </summary>
		public static IServiceCollection ConfigureAllServices(this IServiceCollection services, WebApplicationBuilder builder)
		{
			return services
				.ConfigureSerilog(builder)
				.ConfigureOpenTelemetry(builder)
				.ConfigureControllers()
				.ConfigureSwagger()
				.ConfigureMiddlewareServices()
				.ConfigureKafkaServices()
				.ConfigureHealthChecks(builder.Configuration)
				.ConfigureRedis(builder.Configuration)
				.ConfigureDatabase(builder.Configuration)
				.ConfigureRepositories();
		}

		/// <summary>
		/// Serilog yapılandırmasını ekler
		/// </summary>
		public static IServiceCollection ConfigureSerilog(this IServiceCollection services, WebApplicationBuilder builder)
		{
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
					.WriteTo.Elasticsearch(
						new ElasticsearchSinkOptions(new Uri(context.Configuration["SeriLogConfig:ElasticUri"] ?? "http://elasticsearch:9200"))
						{
							IndexFormat = $"{context.Configuration["SeriLogConfig:ProjectName"]}-{context.Configuration["SeriLogConfig:Environment"]}-logs-{{0:yyyy.MM.dd}}",
							AutoRegisterTemplate = true,
							AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
							ModifyConnectionSettings = conn => conn.BasicAuthentication(
								context.Configuration["SeriLogConfig:ElasticUser"] ?? "elastic",
								context.Configuration["SeriLogConfig:ElasticPassword"] ?? "elastic1234"
							),
							MinimumLogEventLevel = LogEventLevel.Information
						})
					.WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://seq:5341",
						restrictedToMinimumLevel: LogEventLevel.Information);
			});

			return services;
		}

		/// <summary>
		/// OpenTelemetry yapılandırmasını ekler
		/// </summary>
		public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
		{
			services.AddOpenTelemetry() // OpenTelemetry izleme hizmetlerini ekliyoruz.
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

			return services;
		}

		/// <summary>
		/// Controller yapılandırmasını ekler
		/// </summary>
		public static IServiceCollection ConfigureControllers(this IServiceCollection services)
		{
			services.AddControllers(options =>
			{
				// Attribute tabanlı validation yerine FluentValidation kullanılacak
				// FluentValidation exception filter ekle
				options.Filters.Add<FluentValidationExceptionFilter>();
			});

			// API için standart davranışları ayarla - FluentValidation için
			services.Configure<ApiBehaviorOptions>(options =>
			{
				// ValidationProblemDetails devre dışı bırak (kendi exception middleware'imizi kullanacağız)
				options.SuppressModelStateInvalidFilter = true;
			});

			// FluentValidation yapılandırması
			services.AddFluentValidationAutoValidation();
			services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();

			services.AddEndpointsApiExplorer();

			return services;
		}

		/// <summary>
		/// Swagger yapılandırmasını ekler
		/// </summary>
		public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
		{
			services.AddSwaggerServices();
			return services;
		}

		/// <summary>
		/// Middleware servislerini ekler
		/// </summary>
		public static IServiceCollection ConfigureMiddlewareServices(this IServiceCollection services)
		{
			services.AddTransient<RequestResponseLogMiddleware>();
			services.AddTransient<GlobalExceptionHandlerMiddleware>();
			services.AddSingleton<ILogContextEnricher, LogContextEnricher>();

			// HttpClientFactory ekleme (HealthCheck için)
			services.AddHttpClient();

			return services;
		}

		/// <summary>
		/// Kafka servislerini ekler
		/// </summary>
		public static IServiceCollection ConfigureKafkaServices(this IServiceCollection services)
		{
			services.AddSingleton<KafkaProducerService>();
			services.AddHostedService<KafkaConsumerService>();

			return services;
		}

		/// <summary>
		/// HealthCheck servislerini ekler
		/// </summary>
		public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHealthChecks()
				.AddCheck<ElasticsearchHealthCheck>("elasticsearch_health_check",
					tags: new[] { "elasticsearch", "ready" });

			return services;
		}

		/// <summary>
		/// Veritabanı bağlantısını yapılandırır
		/// </summary>
		public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			// Entity Framework'ü SQL Server ile yapılandır
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
					sqlOptions =>
					{
						sqlOptions.EnableRetryOnFailure(
							maxRetryCount: 5,
							maxRetryDelay: TimeSpan.FromSeconds(30),
							errorNumbersToAdd: null);
						sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
					}));

			// HealthCheck için veritabanı kontrolü ekle
			services.AddHealthChecks()
				.AddDbContextCheck<ApplicationDbContext>("database_health_check", tags: new[] { "database", "sql" });

			return services;
		}

		/// <summary>
		/// Repository servislerini yapılandırır
		/// </summary>
		public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
		{
			// Generic repository için DI yapılandırması
			services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

			// Özel repository'ler için DI yapılandırması
			services.AddScoped<ICachedRequestRepository, CachedRequestRepository>();

			// UnitOfWork için DI yapılandırması
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			return services;
		}

		/// <summary>
		/// Redis önbellek servisini yapılandırır
		/// </summary>
		public static IServiceCollection ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
		{
			// Redis yapılandırma seçeneklerini bağla
			var redisOptions = new RedisOptions();
			configuration.GetSection("Redis").Bind(redisOptions);

			// Redis önbellek servisini ekle
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisOptions.ConnectionString;
				options.InstanceName = redisOptions.InstanceName;
			});

			// Cache servisi için bağımlılık enjeksiyonu
			services.AddSingleton<ICacheService, RedisCacheService>();
			
			// HttpContextAccessor (Redis cache için gerekli)
			services.AddHttpContextAccessor();
			
			// Cache temizleme arkaplan servisi
			services.AddHostedService<CacheCleanupService>();

			// Health check için Redis kontrolü ekle
			services.AddHealthChecks()
				.AddRedis(redisOptions.ConnectionString, "redis_health_check", tags: new[] { "redis", "cache" });

			return services;
		}
	}
}