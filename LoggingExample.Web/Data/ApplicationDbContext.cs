using LoggingExample.Web.Data.EntityConfigurations;
using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LoggingExample.Web.Data
{
    /// <summary>
    /// Uygulama veritabanı bağlantı context'i
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// DbContext yapılandırma constructor'ı
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        /// <summary>
        /// Kullanıcılar tablosu
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;
        
        /// <summary>
        /// Loglar tablosu
        /// </summary>
        public DbSet<Log> Logs { get; set; } = null!;
        
        /// <summary>
        /// Önbelleğe alınan istekler tablosu
        /// </summary>
        public DbSet<CachedRequest> CachedRequests { get; set; } = null!;

        /// <summary>
        /// Model oluşturma esnasında çalışan metot
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Entity konfigürasyonlarını uygula
            ApplyEntityConfigurations(modelBuilder);
        }

        /// <summary>
        /// Entity konfigürasyonlarını uygular
        /// </summary>
        private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
        {
            // Tüm entity konfigürasyonlarını bulup uygula
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
} 