using LoggingExample.Web.Data;
using LoggingExample.Web.Models.Entity;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoggingExample.Web.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Lazy yüklenen repository'ler
        private Lazy<IRepository<User>> _userRepository;
        private Lazy<IRepository<Log>> _logRepository;
        private Lazy<ICachedRequestRepository> _cachedRequestRepository;

        public UnitOfWork(
            ApplicationDbContext context, 
            ILogger<UnitOfWork> logger,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = logger;

            // Repository'leri oluştur
            _userRepository = new Lazy<IRepository<User>>(() => 
                new Repository<User>(_context, loggerFactory.CreateLogger<Repository<User>>()));
            
            _logRepository = new Lazy<IRepository<Log>>(() => 
                new Repository<Log>(_context, loggerFactory.CreateLogger<Repository<Log>>()));
            
            _cachedRequestRepository = new Lazy<ICachedRequestRepository>(() => 
                new CachedRequestRepository(_context, loggerFactory.CreateLogger<CachedRequestRepository>()));
        }
        public IRepository<User> UserRepository => _userRepository.Value;

        public IRepository<Log> LogRepository => _logRepository.Value;

        public ICachedRequestRepository CachedRequestRepository => _cachedRequestRepository.Value;

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _logger.LogDebug("Yeni bir transaction başlatılıyor");
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitAsync()
        {
            try
            {
                _logger.LogDebug("Transaction commit ediliyor");
                await _context.SaveChangesAsync();
                
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _logger.LogDebug("Transaction başarıyla commit edildi");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction commit edilirken hata oluştu");
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            _logger.LogDebug("Transaction rollback ediliyor");
            
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    _logger.LogDebug("Transaction başarıyla rollback edildi");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction rollback edilirken hata oluştu");
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            _logger.LogDebug("Değişiklikler veritabanına kaydediliyor");
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Transaction'ı temizle
                    _transaction?.Dispose();
                }

                _disposed = true;
            }
        }
    }
} 