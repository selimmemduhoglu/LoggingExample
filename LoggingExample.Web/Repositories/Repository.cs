using LoggingExample.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LoggingExample.Web.Repositories
{
    /// <summary>
    /// Generic repository implementasyonu
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        /// <summary>
        /// Repository constructor
        /// </summary>
        public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Getting entity of type {EntityType} with id {Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            _logger.LogDebug("Getting all entities of type {EntityType}", typeof(T).Name);
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            _logger.LogDebug("Finding entities of type {EntityType} with filter", typeof(T).Name);
            return await _dbSet.Where(filter).ToListAsync();
        }
        public async Task AddAsync(T entity)
        {
            _logger.LogDebug("Adding new entity of type {EntityType}", typeof(T).Name);
            await _dbSet.AddAsync(entity);
            // SaveChangesAsync artık UnitOfWork tarafından çağrılacak
        }

        public async Task UpdateAsync(T entity)
        {
            _logger.LogDebug("Updating entity of type {EntityType}", typeof(T).Name);
            _dbSet.Update(entity);
            // SaveChangesAsync artık UnitOfWork tarafından çağrılacak
            await Task.CompletedTask; // Async metot olduğu için boş task dönüyoruz
        }

        public async Task DeleteAsync(T entity)
        {
            _logger.LogDebug("Deleting entity of type {EntityType}", typeof(T).Name);
            _dbSet.Remove(entity);
            // SaveChangesAsync artık UnitOfWork tarafından çağrılacak
            await Task.CompletedTask; // Async metot olduğu için boş task dönüyoruz
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting entity of type {EntityType} with id {Id}", typeof(T).Name, id);
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            _logger.LogDebug("Checking if entity of type {EntityType} exists with filter", typeof(T).Name);
            return await _dbSet.AnyAsync(filter);
        }

        public async Task<int> CountAsync()
        {
            _logger.LogDebug("Counting all entities of type {EntityType}", typeof(T).Name);
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            _logger.LogDebug("Counting entities of type {EntityType} with filter", typeof(T).Name);
            return await _dbSet.CountAsync(filter);
        }
    }
} 