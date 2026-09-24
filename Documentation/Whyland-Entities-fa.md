[🇬🇧 English Version](./Whyland-Entities.md)

# موجودیت‌های (Entities) پروژه Whyland

این فایل تمام Entity های اصلی مورد نیاز برای پیاده‌سازی پروژه Whyland بر اساس سند SRS را شامل می‌شود. هر بخش شامل نام Entity و کد کلاس C# مربوط به آن است.
---
## 1. User

Entity احراز هویت کاربر (بدون استفاده از ASP.NET Core Identity).

```csharp
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
    public InstructorProfile? InstructorProfile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<ExternalLogin> ExternalLogins { get; set; }
    public ICollection<UserEpisodeProgress> EpisodeProgresses { get; set; }
}
```
---
## 2. UserProfile

اطلاعات پروفایل تکمیلی کاربر که از اطلاعات احراز هویت جدا نگه‌داری می‌شود.

```csharp
public class UserProfile
{
    public Guid UserId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; }
}
```
---
## 3. InstructorProfile

اطلاعات اختصاصی مدرس که یک User را گسترش می‌دهد.

```csharp
public class InstructorProfile
{
    public Guid UserId { get; set; }
    public string? ProfileImage { get; set; }
    public string? Bio { get; set; }
    public string? Biography { get; set; }
    public string? Specialization { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Instagram { get; set; }
    public string? Address { get; set; }
    public string? ShebaNumber { get; set; }
    public string? BankAccountNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; }
    public ICollection<CourseInstructor> CourseInstructors { get; set; }
}
```
---
## 4. ExternalLogin

رکورد اتصال حساب کاربری محلی به هویت خارجی (Google) — از بخش احراز هویت گوگل استنتاج شده است.

```csharp
public class ExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; }        // e.g. "Google"
    public string ProviderKey { get; set; }      // external provider's unique user id
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}
```
---
## 5. Role

نقش‌های پویا (Dynamic Roles).

```csharp
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```
---
## 6. Permission

مجوزهای پویا (Dynamic Permissions) که با کلید یکتا شناسایی می‌شوند.

```csharp
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; }
}
```
---
## 7. UserRole

جدول واسط برای تخصیص نقش به کاربر.

```csharp
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User User { get; set; }
    public Role Role { get; set; }
}
```
---
## 8. RolePermission

جدول واسط برای تخصیص مجوز به نقش.

```csharp
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public Role Role { get; set; }
    public Permission Permission { get; set; }
}
```
---
## 9. Category

دسته‌بندی سلسله‌مراتبی دوره‌ها (حداکثر سه سطح).

```csharp
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LatinName { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; }
    public ICollection<CourseCategory> CourseCategories { get; set; }
}
```
---
## 10. Course

موجودیت اصلی دوره آموزشی.

```csharp
public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LatinName { get; set; }
    public string ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; }
    public string? MainImage { get; set; }
    public decimal? Price { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public bool IsSaleActive { get; set; }
    public DisplayStatus DisplayStatus { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsAmazing { get; set; }
    public string? IntroductionVideo { get; set; }
    public TimeSpan? TotalDurationMinutes { get; set; }
    public int? ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<CourseCategory> CourseCategories { get; set; }
    public ICollection<CourseInstructor> CourseInstructors { get; set; }
    public ICollection<CourseSection> CourseSections { get; set; }
    public ICollection<CourseFaq> CourseFaqs { get; set; }
}
```

### Enum های مرتبط با Course

```csharp
public enum DiscountType
{
    None,
    Percentage,
    FixedAmount
}
``````csharp
public enum DisplayStatus
{
    Draft,
    Published,
    Hidden
}
```
---
## 11. CourseCategory

جدول واسط بین Course و Category (رابطه چند به چند).

```csharp
public class CourseCategory
{
    public Guid CourseId { get; set; }
    public Guid CategoryId { get; set; }

    public Course Course { get; set; }
    public Category Category { get; set; }
}
```
---
## 12. CourseInstructor

جدول واسط بین Course و InstructorProfile (رابطه چند به چند).

```csharp
public class CourseInstructor
{
    public Guid CourseId { get; set; }
    public Guid InstructorUserId { get; set; }

    public Course Course { get; set; }
    public InstructorProfile Instructor { get; set; }
}
```
---
## 13. CourseSection

بخش‌های تشکیل‌دهنده یک دوره.

```csharp
public class CourseSection
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Course Course { get; set; }
    public ICollection<Episode> Episodes { get; set; }
}
```
---
## 14. Episode

قسمت‌های آموزشی داخل هر بخش (Section).

```csharp
public class Episode
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Video { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool IsFree { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public CourseSection Section { get; set; }
    public ICollection<EpisodeAttachment> Attachments { get; set; }
    public ICollection<UserEpisodeProgress> UserProgresses { get; set; }
}
```
---
## 15. EpisodeAttachment

فایل‌های پیوست هر قسمت (Episode) با محدودیت‌های قابل تنظیم.

```csharp
public class EpisodeAttachment
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }

    public Episode Episode { get; set; }
}
```
---
## 16. UserEpisodeProgress

وضعیت مشاهده و پیشرفت هر اپیزود را به‌صورت مستقل برای هر کاربر نگه‌داری می‌کند. این Entity رابطه بین یک کاربر مشخص و یک اپیزود مشخص را مدل می‌کند.

```csharp
public class UserEpisodeProgress
{
    public Guid UserId { get; set; }
    public Guid EpisodeId { get; set; }
    public EpisodeWatchStatus Status { get; set; }
    public TimeSpan? LastWatchedPosition { get; set; }   // last playback position
    public DateTime? FirstWatchedAt { get; set; }        // when the user first started this episode
    public DateTime? CompletedAt { get; set; }           // when the user fully watched it
    public DateTime? LastWatchedAt { get; set; }         // last time the user watched it
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; }
    public Episode Episode { get; set; }
}
```

### Enum مرتبط با UserEpisodeProgress

```csharp
public enum EpisodeWatchStatus
{
    NotWatched,
    InProgress,
    Watched,
    Locked
}
```
---
## 17. CourseFaq

سوالات متداول اختصاصی هر دوره.

```csharp
public class CourseFaq
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Course Course { get; set; }
}
```
---
## 18. FavoriteCourse

علاقه‌مندی‌های کاربر به دوره‌ها.

```csharp
public class FavoriteCourse
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public Course Course { get; set; }
}
```
---
## 19. UserCourse

دسترسی کاربر به دوره خریداری‌شده (Enrollment).

```csharp
public class UserCourse
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime PurchasedAt { get; set; }

    public User User { get; set; }
    public Course Course { get; set; }
    public Order Order { get; set; }
}
```
---
## 20. Cart and CartItem (Redis Shopping Cart Concept)

سبد خرید در Redis نگهداری می‌شود و Entity پایگاه‌داده محسوب نمی‌شود. این کلاس‌ها صرفاً مدل‌های داده‌ای هستند که به‌صورت JSON در Redis ذخیره می‌شوند (کلید: `cart:{userId}`).

سبد خرید حداکثر یک کوپن اعمال‌شده را با کد آن نگه‌داری می‌کند. مبلغ تخفیف در سبد ذخیره نمی‌شود و سرور باید آن را در هر بار خواندن مجدداً محاسبه کند.

```csharp
public class Cart
{
    public List<CartItem> Items { get; set; } = [];
    public string? CouponCode { get; set; }
}

public class CartItem
{
    public Guid CourseId { get; set; }
    public int Quantity { get; set; }
}
```
---
## 21. Order

سفارش ایجادشده از سبد خرید.

```csharp
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }              // set while the order is Pending
    public Guid? CouponId { get; set; }
    public decimal CouponDiscountAmount { get; set; }     // 0 when no coupon is applied

    public User User { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
```

### Enum مرتبط با Order

```csharp
public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled,
    Failed
}
```
---
## 22. OrderItem

اقلام هر سفارش (هر دوره داخل سفارش).

```csharp
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CourseId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Order Order { get; set; }
    public Course Course { get; set; }
}
```
---
## 23. PaymentMethod (Enum)

روش‌های پرداخت به صورت Enum.

```csharp
public enum PaymentMethod
{
    BankGateway,
    ZarinPal,
    SnappPay
}
```
---
## 24. Payment

تراکنش مالی مرتبط با یک سفارش.

```csharp
public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public Order Order { get; set; }
}
```

### Enum مرتبط با Payment

```csharp
public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Cancelled
}
```
---
## 25. Invoice

سند مالی نهایی مرتبط با سفارش پرداخت‌شده.

```csharp
public class Invoice
{
    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; }
    public DateTime InvoiceDate { get; private set; }

    public Guid OrderId { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid UserId { get; private set; }

    public string CustomerName { get; private set; }
    public string CustomerPhone { get; private set; }
    public string CustomerEmail { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }         // includes the coupon discount
    public string? CouponCode { get; private set; }       // snapshot, null when no coupon was used
    public decimal CouponDiscount { get; private set; }   // snapshot, 0 when no coupon was used
    public decimal Tax { get; private set; }
    public decimal FinalAmount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public Order Order { get; private set; }
    public User User { get; private set; }
}
```
---
## 26. InvoiceItem

آیتم های فاکتور.

```csharp
public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid CourseId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalPrice { get; private set; }
}
```
---
## 27. BlogPost

مقالات وبلاگ عمومی سایت.

```csharp
public class BlogPost
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Image { get; set;}
    public int DisplayOrder { get; set; }
    public DisplayStatus DisplayStatus { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public User Author { get; set; }
    public BlogCategory? Category { get; set; }
}
```
---
## 28. BlogCategory

دسته‌بندی سلسله‌مراتبی مطالب وبلاگ.

```csharp
public class BlogCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LatinName { get; set; }
    public string Slug { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public BlogCategory? Parent { get; set; }
    public ICollection<BlogCategory> Children { get; set; }
    public ICollection<BlogPost> BlogPosts { get; set; }
}
```
---
## 29. Faq

سوالات متداول عمومی سایت (مجزا از CourseFaq).

```csharp
public class Faq
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
```
---
## 30. SiteSetting

تنظیمات پویای سایت (کلید/مقدار) — عمومی، سئو و اطلاعات تماس.

```csharp
public class SiteSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string? Value { get; set; }
    public string Group { get; set; }   // e.g. "General", "SEO", "Contact"
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
```
---
## 31. OtpRequest

رکورد درخواست/تایید کد یک‌بار مصرف برای احراز هویت با شماره تلفن — از بخش احراز هویت تلفنی استنتاج شده است.

```csharp
public class OtpRequest
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; }
    public string CodeHash { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
```
---
## 32. PasswordResetToken

توکن بازیابی رمز عبور — از بخش «فراموشی رمز عبور» استنتاج شده است.

```csharp
public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}
```

## 33. Coupon

کد تخفیف برای سفارشات.

```csharp
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}

public enum CouponType
{
    Percentage,
    FixedAmount
}
```
