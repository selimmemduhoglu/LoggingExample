using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoggingExample.Web.Data.EntityConfigurations
{
    /// <summary>
    /// Entity Type konfigürasyonları için taban sınıf
    /// </summary>
    /// <typeparam name="TEntity">Konfigüre edilecek entity tipi</typeparam>
    public abstract class EntityTypeConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Entity konfigürasyonunu uygular
        /// </summary>
        /// <param name="builder">Entity type builder</param>
        public abstract void Configure(EntityTypeBuilder<TEntity> builder);
    }
} 