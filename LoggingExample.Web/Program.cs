using LoggingExample.Web.Configurations;
using LoggingExample.Web.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Tüm servisleri ekleme
builder.Services.ConfigureAllServices(builder);

var app = builder.Build();

try
{
    // Veritabanını başlat
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DatabaseInitializer");
    DatabaseInitializer.Initialize(app.Services, logger);
}
catch (Exception ex)
{
    // Hata olursa loglama yap ama uygulamayı çalıştırmaya devam et
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Startup");
    logger.LogError(ex, "Uygulama başlatılırken bir hata oluştu");
}

// Tüm middleware'leri yapılandırma
app.ConfigureAllMiddlewares(builder.Configuration);

app.Run();