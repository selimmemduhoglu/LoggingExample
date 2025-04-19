using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoggingExample.Web.Data.EntityConfigurations
{
    /// <summary>
    /// CachedRequest entity için konfigürasyon sınıfı
    /// </summary>
    public class CachedRequestConfiguration : EntityTypeConfigurationBase<CachedRequest>
    {
        /// <summary>
        /// CachedRequest entity konfigürasyonunu uygular
        /// </summary>
        public override void Configure(EntityTypeBuilder<CachedRequest> builder)
        {
            // Tablo adı ve şema tanımı
            builder.ToTable("CachedRequests");

            // Primary key tanımı
            builder.HasKey(c => c.Id);

            // Property konfigürasyonları
            builder.Property(c => c.CacheKey)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.RequestUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(c => c.HttpMethod)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("GET");

            builder.Property(c => c.CachedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.LastAccessed)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.HitCount)
                .HasDefaultValue(0);

            builder.Property(c => c.CorrelationId)
                .HasMaxLength(50);

            // Indexler
            builder.HasIndex(c => c.CacheKey)
                .IsUnique();

            builder.HasIndex(c => c.ExpiresAt);
        }
    }
} 