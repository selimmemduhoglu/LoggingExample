using LoggingExample.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Tüm servisleri ekleme
builder.Services.ConfigureAllServices(builder);

var app = builder.Build();

// Tüm middleware'leri yapılandırma
app.ConfigureAllMiddlewares(builder.Configuration);

app.Run();