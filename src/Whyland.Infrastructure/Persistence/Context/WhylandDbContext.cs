using System.Linq.Expressions;
using Whyland.Domain.Common;
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
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<CourseInstructor> CourseInstructors => Set<CourseInstructor>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<EpisodeAttachment> EpisodeAttachments => Set<EpisodeAttachment>();
    public DbSet<CourseFaq> CourseFaqs => Set<CourseFaq>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    public DbSet<UserEpisodeProgress> UserEpisodeProgresses => Set<UserEpisodeProgress>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<FavoriteCourse> FavoriteCourses => Set<FavoriteCourse>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Faq> Faqs => Set<Faq>();

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
