using System.Linq.Expressions;
using Whyland.Domain.Common.Primitives;
using Whyland.Domain.Entities.Content;
using Whyland.Domain.Entities.Interactions;
using Whyland.Domain.Entities.Settings;

namespace Whyland.Infrastructure.Persistence.Context;

/// <summary>
/// The application's EF Core DbContext. It implements the Unit of Work behavior (s3.2.2)
/// and must never be exposed outside the Infrastructure layer (s3.4.3).
/// Entity mapping lives in dedicated IEntityTypeConfiguration classes under
/// <c>Persistence/Configurations</c>; this context only composes them and applies
/// cross-cutting behaviors such as the soft-delete global query filter.
/// </summary>
public class WhylandDbContext(DbContextOptions<WhylandDbContext> options)
    : DbContext(options)
{
    /// <summary>Registered application users.</summary>
    public DbSet<User> Users => Set<User>();
    /// <summary>Customer-facing profile data attached one-to-one to a user.</summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    /// <summary>Instructor profile data attached one-to-one to a user.</summary>
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    /// <summary>One-time password codes issued for verification flows.</summary>
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    /// <summary>Single-use tokens issued for password reset flows.</summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    /// <summary>External (social) login identities linked to a user.</summary>
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    /// <summary>System roles assignable to users and permissions.</summary>
    public DbSet<Role> Roles => Set<Role>();
    /// <summary>Fine-grained authorization permissions with unique keys.</summary>
    public DbSet<Permission> Permissions => Set<Permission>();
    /// <summary>Links between users and their assigned roles.</summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    /// <summary>Links between roles and their granted permissions.</summary>
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    /// <summary>Hierarchical categories used to organize courses.</summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>Buyable course aggregate roots.</summary>
    public DbSet<Course> Courses => Set<Course>();
    /// <summary>Links between courses and categories.</summary>
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    /// <summary>Links between courses and their instructors.</summary>
    public DbSet<CourseInstructor> CourseInstructors => Set<CourseInstructor>();
    /// <summary>Sections that group a course's episodes in display order.</summary>
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    /// <summary>Playable video episodes inside course sections.</summary>
    public DbSet<Episode> Episodes => Set<Episode>();
    /// <summary>Downloadable file attachments belonging to an episode.</summary>
    public DbSet<EpisodeAttachment> EpisodeAttachments => Set<EpisodeAttachment>();
    /// <summary>FAQ items shown on a course page.</summary>
    public DbSet<CourseFaq> CourseFaqs => Set<CourseFaq>();
    /// <summary>Course orders placed by users.</summary>
    public DbSet<Order> Orders => Set<Order>();
    /// <summary>Purchasable course lines inside an order.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    /// <summary>Printable purchase invoices with customer/money snapshots.</summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();
    /// <summary>Invoiced course lines with price snapshots.</summary>
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    /// <summary>Payment attempts against an order.</summary>
    public DbSet<Payment> Payments => Set<Payment>();
    /// <summary>Discount coupons applicable to orders.</summary>
    public DbSet<Coupon> Coupons => Set<Coupon>();
    /// <summary>Purchase records granting a user permanent access to a course.</summary>
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    /// <summary>Per-user playback progress of a single episode.</summary>
    public DbSet<UserEpisodeProgress> UserEpisodeProgresses => Set<UserEpisodeProgress>();
    /// <summary>Blog posts published by authors.</summary>
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    /// <summary>Hierarchical categories organizing blog posts.</summary>
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    /// <summary>User favorites ("wishlist") linking users to courses.</summary>
    public DbSet<FavoriteCourse> FavoriteCourses => Set<FavoriteCourse>();
    /// <summary>Editable site configuration entries, keyed by name.</summary>
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    /// <summary>Standalone FAQ items shown in the general-purpose FAQ section.</summary>
    public DbSet<Faq> Faqs => Set<Faq>();

    /// <summary>
    /// Composes entity mappings from the configuration classes in this assembly
    /// and applies the global soft-delete query filter to every ISoftDeletable entity type.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to shape the persistence model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // All table, property, key, index and relation mapping comes from the
        // configuration classes in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WhylandDbContext).Assembly);

        // Soft-deleted entities must not be returned by ordinary queries (s4.1.2):
        // global query filters are applied automatically for every ISoftDeletable entity type.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            var filter = BuildSoftDeleteFilter(entityType.ClrType);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    /// <summary>
    /// Builds the expression <c>e =&gt; e.IsDeleted == false</c> for the given entity type.
    /// </summary>
    /// <param name="clrType">The CLR entity type to filter.</param>
    /// <returns>The lambda expression used as the global query filter.</returns>
    private static LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var body = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(body, parameter);
    }
}
