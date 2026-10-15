
namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

public sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("InstructorProfiles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.ProfileImage)
            .HasMaxLength(256);

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        builder.Property(x => x.Specialization)
            .HasMaxLength(200);

        builder.Property(x => x.Website)
            .HasMaxLength(256);

        builder.Property(x => x.LinkedIn)
            .HasMaxLength(256);

        builder.Property(x => x.Instagram)
            .HasMaxLength(256);

        builder.Property(x => x.NationalCode)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.ShebaNumber)
            .HasMaxLength(26);

        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(26);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        #endregion

        #region Relations

        builder.HasMany(x => x.CourseInstructors)
            .WithOne(x => x.InstructorProfile)
            .HasForeignKey(x => x.InstructorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
