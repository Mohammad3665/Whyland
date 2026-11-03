using Whyland.Domain.Common.Entities;

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

        // The key column is an identifier, so the type the database generates for is the raw key it
        // wraps, never the wrapper struct. Read off the unwrapped type: matched on typeof(TKey) a
        // wrapped key reaches neither branch below and PostgreSQL is left with a plain bigint or
        // uuid column — no identity, no default, every insert having to supply the primary key.
        var key = StronglyTypedIds.Unwrap(typeof(TKey));

        if (key == typeof(Guid))
        {
            builder
                .Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        }
        else if (key == typeof(int) || key == typeof(long))
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        }
    }
}
