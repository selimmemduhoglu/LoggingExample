namespace LoggingExample.Web.Middlewares;

public class RequestResponseLogMiddleware(ILoggerFactory loggerFactory) : IMiddleware
{

    private readonly ILogger<RequestResponseLogMiddleware> _logger = loggerFactory.CreateLogger<RequestResponseLogMiddleware>();
    //- ILoggerFactory'den bu middleware'e özel bir logger oluşturuyor.


    // HttpContext parametresi: Mevcut HTTP isteği hakkında tüm bilgileri içerir (istek, yanıt, kullanıcı bilgileri vb.)
    // RequestDelegate next: Pipeline'daki bir sonraki middleware'i temsil eder
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Test Log {RequestPath}", context.Request.Path); //Gelen isteğin yolunu (path) loglar
        await next(context); //Pipeline'daki bir sonraki middleware'i çağırır, böylece istek işlemeye devam eder
    }
}