namespace Whyland.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// Maps the extended instructor profile attached to a user one-to-one (s5.4).
/// </summary>
public sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("InstructorProfiles", SchemaConstants.Identity);

        #region Properties

        builder.Property(x => x.ProfileImage)
            .HasMaxLength(InstructorProfile.MaxProfileImageLength);

        builder.Property(x => x.Bio)
            .HasMaxLength(InstructorProfile.MaxBioLength);

        builder.Property(x => x.Specialization)
            .HasMaxLength(InstructorProfile.MaxSpecializationLength);

        builder.Property(x => x.Website)
            .HasMaxLength(InstructorProfile.MaxWebsiteLength);

        builder.Property(x => x.LinkedIn)
            .HasMaxLength(InstructorProfile.MaxLinkedInLength);

        builder.Property(x => x.Instagram)
            .HasMaxLength(InstructorProfile.MaxInstagramLength);

        builder.Property(x => x.ShebaNumber)
            .HasMaxLength(InstructorProfile.MaxShebaNumberLength);

        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(InstructorProfile.MaxBankAccountNumberLength);

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
