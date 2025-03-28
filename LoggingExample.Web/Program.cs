using Elastic.Apm.NetCoreAll;
using LoggingExample.Web.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<RequestResponseLogMiddleware>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAllElasticApm(builder.Configuration);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseMiddleware<RequestResponseLogMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.UseHealthChecks("/api/health-check");

app.Run();