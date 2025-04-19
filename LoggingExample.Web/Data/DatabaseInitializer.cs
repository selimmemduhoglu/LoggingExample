using Microsoft.EntityFrameworkCore;

namespace LoggingExample.Web.Data
{
    /// <summary>
    /// Veritabanı başlatıcı sınıfı
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Uygulama başlangıcında veritabanını hazırlar
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                logger.LogInformation("Veritabanı migrasyonları kontrol ediliyor");
                
                // Veritabanını oluştur ve migrations'ları uygula
                dbContext.Database.Migrate();
                
                logger.LogInformation("Veritabanı başarıyla hazırlandı");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Veritabanı hazırlanırken bir hata oluştu");
                throw;
            }
        }
    }
} 