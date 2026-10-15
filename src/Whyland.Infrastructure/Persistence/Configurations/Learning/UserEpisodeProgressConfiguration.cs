
namespace Whyland.Infrastructure.Persistence.Configurations.Learning;

/// <summary>
/// Maps the per-user playback progress of a single episode (s4.7).
/// </summary>
public sealed class UserEpisodeProgressConfiguration : IEntityTypeConfiguration<UserEpisodeProgress>
{
    public void Configure(EntityTypeBuilder<UserEpisodeProgress> builder)
    {
        builder.ToTable("UserEpisodeProgresses", SchemaConstants.Learning);

        #region Keys

        // A user tracks each episode exactly once
        builder.HasKey(x => new { x.UserId, x.EpisodeId });

        #endregion

        #region Properties

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(EpisodeWatchStatus.NotWatched);

        #endregion

        #region Indexes

        // Progress listings are filtered/watch-ordered by user (s4.7)
        builder.HasIndex(x => new { x.UserId, x.LastWatchedAt });

        builder.HasIndex(x => new { x.UserId, x.Status });

        #endregion

        #region Relations

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Progress is meaningless without its episode and must follow its removal
        builder.HasOne(x => x.Episode)
            .WithMany()
            .HasForeignKey(x => x.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
