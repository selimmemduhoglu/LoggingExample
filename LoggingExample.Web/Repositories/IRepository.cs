using System.Linq.Expressions;

namespace LoggingExample.Web.Repositories
{
    /// <summary>
    /// Generic repository pattern için temel arayüz
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Belirtilen ID'ye sahip entity'yi getirir
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Bulunan entity veya null</returns>
        Task<T?> GetByIdAsync(int id);
        
        /// <summary>
        /// Tüm entity'leri getirir
        /// </summary>
        /// <returns>Entity listesi</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Belirtilen koşula göre filtreleme yapar
        /// </summary>
        /// <param name="filter">Filtre ifadesi</param>
        /// <returns>Filtrelenmiş entity listesi</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);
        
        /// <summary>
        /// Yeni entity ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        /// <returns>İşlem sonucu</returns>
        Task AddAsync(T entity);
        
        /// <summary>
        /// Entity'yi günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        /// <returns>İşlem sonucu</returns>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Entity'yi siler
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        /// <returns>İşlem sonucu</returns>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Belirtilen ID'ye sahip entity'yi siler
        /// </summary>
        /// <param name="id">Silinecek entity ID</param>
        /// <returns>İşlem sonucu</returns>
        Task DeleteAsync(int id);
        
        /// <summary>
        /// Belirtilen koşula uyan entity var mı kontrolü yapar
        /// </summary>
        /// <param name="filter">Kontrol edilecek koşul</param>
        /// <returns>Koşula uyan entity varsa true, yoksa false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        
        /// <summary>
        /// Toplam entity sayısını döndürür
        /// </summary>
        /// <returns>Entity sayısı</returns>
        Task<int> CountAsync();
        
        /// <summary>
        /// Belirtilen koşula uyan entity sayısını döndürür
        /// </summary>
        /// <param name="filter">Filtre ifadesi</param>
        /// <returns>Koşula uyan entity sayısı</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> filter);
    }
} 