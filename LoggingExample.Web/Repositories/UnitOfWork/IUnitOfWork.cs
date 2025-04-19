using LoggingExample.Web.Models.Entity;

namespace LoggingExample.Web.Repositories.UnitOfWork
{
    /// <summary>
    /// Veritabanı işlemlerini bir transaction içinde yönetmek için UnitOfWork arayüzü
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Kullanıcı repository'si
        /// </summary>
        IRepository<User> UserRepository { get; }
        
        /// <summary>
        /// Log repository'si
        /// </summary>
        IRepository<Log> LogRepository { get; }
        
        /// <summary>
        /// Önbelleklenen istekler repository'si
        /// </summary>
        ICachedRequestRepository CachedRequestRepository { get; }
        
        /// <summary>
        /// Yapılan değişiklikleri veritabanına kaydeder
        /// </summary>
        /// <returns>Etkilenen satır sayısı</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Yeni bir transaction başlatır
        /// </summary>
        /// <returns>Transaction nesnesi</returns>
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
        
        /// <summary>
        /// Transaction'ı onaylar (commit)
        /// </summary>
        Task CommitAsync();
        
        /// <summary>
        /// Transaction'ı geri alır (rollback)
        /// </summary>
        Task RollbackAsync();
    }
} 