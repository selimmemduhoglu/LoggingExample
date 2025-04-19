using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoggingExample.Web.Data.EntityConfigurations
{
    /// <summary>
    /// Log entity için konfigürasyon sınıfı
    /// </summary>
    public class LogConfiguration : EntityTypeConfigurationBase<Log>
    {
        /// <summary>
        /// Log entity konfigürasyonunu uygular
        /// </summary>
        public override void Configure(EntityTypeBuilder<Log> builder)
        {
            // Tablo adı ve şema tanımı
            builder.ToTable("Logs");

            // Primary key tanımı
            builder.HasKey(l => l.Id);

            // Property konfigürasyonları
            builder.Property(l => l.Level)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(l => l.Message)
                .IsRequired();

            builder.Property(l => l.Source)
                .HasMaxLength(100);

            builder.Property(l => l.RequestId)
                .HasMaxLength(50);

            builder.Property(l => l.CorrelationId)
                .HasMaxLength(50);

            builder.Property(l => l.IpAddress)
                .HasMaxLength(50);

            builder.Property(l => l.UserAgent)
                .HasMaxLength(500);

            builder.Property(l => l.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexler
            builder.HasIndex(l => l.Timestamp);
            
            builder.HasIndex(l => l.Level);
            
            builder.HasIndex(l => l.CorrelationId);
        }
    }
} 