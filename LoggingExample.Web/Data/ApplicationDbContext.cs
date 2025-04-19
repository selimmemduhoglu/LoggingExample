using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace LoggingExample.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        
        public DbSet<Log> Logs { get; set; } = null!;
        
        public DbSet<CachedRequest> CachedRequests { get; set; } = null!;

        /// <summary>
        /// Model oluşturma esnasında çalışan metot
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users için indexler
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Logs için indexler
            modelBuilder.Entity<Log>()
                .HasIndex(l => l.Timestamp);
            
            modelBuilder.Entity<Log>()
                .HasIndex(l => l.Level);
            
            modelBuilder.Entity<Log>()
                .HasIndex(l => l.CorrelationId);
            
            // CachedRequests için indexler
            modelBuilder.Entity<CachedRequest>()
                .HasIndex(c => c.CacheKey)
                .IsUnique();
            
            modelBuilder.Entity<CachedRequest>()
                .HasIndex(c => c.ExpiresAt);
            
            // Seed data - Admin kullanıcı
            modelBuilder.Entity<User>().HasData(
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