using Whyland.Domain.Entities.Common;

namespace Whyland.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Provides the shared base key configuration (primary key and value generation)
/// inherited by every entity configuration (s3.8).
/// </summary>
public sealed class BaseConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity<TKey>
    where TKey : notnull
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        if (typeof(TKey) == typeof(Guid))
        {
            builder
                .Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        }
        else if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(long))
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        }
    }
}
