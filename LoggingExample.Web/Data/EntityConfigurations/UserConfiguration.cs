using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoggingExample.Web.Data.EntityConfigurations
{
    /// <summary>
    /// User entity için konfigürasyon sınıfı
    /// </summary>
    public class UserConfiguration : EntityTypeConfigurationBase<User>
    {
        /// <summary>
        /// User entity konfigürasyonunu uygular
        /// </summary>
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            // Tablo adı ve şema tanımı
            builder.ToTable("Users");

            // Primary key tanımı
            builder.HasKey(u => u.Id);

            // Property konfigürasyonları
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexler
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.HasIndex(u => u.Email)
                .IsUnique();
                
            // Seed data
            builder.HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@loggingexample.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEHlVMRYwP5JWtNnlOOvpEMWtHLbsREKgfS1VpbUw2uEQqIk99gXoXSQ4uR7ERlj32w==", // Password: Admin123!
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
} 