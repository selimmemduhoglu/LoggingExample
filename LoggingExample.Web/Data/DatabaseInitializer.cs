using Microsoft.EntityFrameworkCore;

namespace LoggingExample.Web.Data
{
    /// <summary>
    /// Veritabanı başlatıcı sınıfı
    /// </summary>
    /// <remarks>
    /// Bu sınıf, uygulama başlatıldığında otomatik olarak veritabanını kontrol eder ve gerekirse 
    /// bekleyen migrasyonları uygular. Bu, uygulama geliştirme sürecinde kolaylık sağlar.
    /// 
    /// Geliştirme sürecinde yeni bir model değişikliği yapıldığında:
    /// 1. `dotnet ef migrations add MigrationName` komutu ile yeni migrasyon oluşturulmalıdır.
    /// 2. Ardından uygulama yeniden başlatıldığında, `Database.Migrate()` metodu otomatik olarak 
    ///    bu migrasyonu veritabanına uygular.
    /// 
    /// Üretim ortamına geçmeden önce:
    /// 1. Tüm migrasyonlar test edilmelidir.
    /// 2. Üretim veritabanına uygulanmadan önce, gerekirse migrasyonlar düzenlenebilir veya
    ///    `dotnet ef migrations script` komutu ile SQL script olarak export edilebilir.
    /// </remarks>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Uygulama başlangıcında veritabanını hazırlar
        /// </summary>
        /// <remarks>
        /// Bu metot, veritabanını kontrol eder ve gerekirse migrasyonları uygular.
        /// Connection String, appsettings.json dosyasında belirtilen "DefaultConnection" 
        /// üzerinden Windows Authentication kullanılarak SQL Server'a bağlanır.
        /// </remarks>
        public static void Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                logger.LogInformation("Veritabanı migrasyonları kontrol ediliyor");
				// dotnet ef migrations add InitialMigration> --project LoggingExample.Web
                // command'ını uygula ve sonrasında aşağıda ki Migration ile Uğpdate.Database işlemi yapılıyor.
				// Veritabanını oluştur ve migrations'ları uygula
				dbContext.Database.Migrate(); // Veritabanını oluştur ve migrasyonları uygula

				logger.LogInformation("Veritabanı başarıyla hazırlandı");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Veritabanı hazırlanırken bir hata oluştu. Migration'ları manuel olarak uygulamanız gerekebilir.");
                throw;
            }
        }
    }
} 