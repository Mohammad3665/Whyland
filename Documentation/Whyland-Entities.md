[🇮🇷 نسخه فارسی](./Whyland-Entities-fa.md)

# Whyland Project Entities

This file contains all the main Entities required for implementing the Whyland project based on the SRS document. Each section includes the Entity name and its corresponding C# class code.

---

## 1. User

User authentication entity (without using ASP.NET Core Identity).

```csharp
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
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
}
```

---

## 2. UserProfile

Additional user profile information that is stored separately from authentication information.

```csharp
public class UserProfile
{
    public Guid UserId { get; set; }
    public string? NationalCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; }
}
```

---

## 3. InstructorProfile

Instructor-specific information that extends a User.

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
    public string? NationalCode { get; set; }
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

A record that connects a local user account to an external identity provider (Google), inferred from the Google authentication section.

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

Dynamic roles.

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

Dynamic permissions identified by a unique key.

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

Join table for assigning roles to users.

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

Join table for assigning permissions to roles.

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

Hierarchical course categorization with a maximum of three levels.

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

The main educational course entity.

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

### Enums Related to Course

```csharp
public enum DiscountType
{
    None,
    Percentage,
    FixedAmount
}
```

```csharp
public enum DisplayStatus
{
    Draft,
    Published,
    Hidden
}
```

---

## 11. CourseCategory

Join table between Course and Category (many-to-many relationship).

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

Join table between Course and InstructorProfile (many-to-many relationship).

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

The sections that make up a course.

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

Educational episodes within each section.

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
}
```

---

## 15. EpisodeAttachment

Attachments for each episode with configurable restrictions.

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

## 16. CourseFaq

Course-specific frequently asked questions.

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

## 17. FavoriteCourse

User's favorite courses.

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

## 18. UserCourse

User's access to a purchased course (Enrollment).

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

## 19. Cart and CartItem (Redis Shopping Cart Concept)

The shopping cart is stored in Redis and is not considered a database Entity. These classes are only data models that are stored as JSON in Redis (key: `cart:{userId}`).

The cart carries at most one applied coupon, identified by its code only. Discount amounts are never stored in the cart; the server recalculates them on every read.

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

## 20. Order

An order created from the shopping cart.

```csharp
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }              // payable amount after the coupon discount
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

### Enum Related to Order

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

## 21. OrderItem

Items within an order (each course included in the order).

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

## 22. PaymentMethod (Enum)

Payment methods represented as an enum.

```csharp
public enum PaymentMethod
{
    BankGateway,
    ZarinPal,
    SnappPay
}
```

---

## 23. Payment

Financial transaction associated with an order.

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

### Enum Related to Payment

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

## 24. Invoice

The final financial document associated with a paid order.

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

## 25. InvoiceItem

The invoice Items.

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

## 26. BlogPost

Public blog articles on the website.

```csharp
public class BlogPost
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public string? Image { get; set; }
    public int DisplayOrder { get; set; }
    public DisplayStatus DisplayStatus { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public User Author { get; set; }
}
```

---

## 27. Faq

General website frequently asked questions (separate from CourseFaq).

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

## 28. SiteSetting

Dynamic website settings (key/value) — general settings, SEO, and contact information.

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

## 29. OtpRequest

A record for requesting/verifying a one-time password for phone-number authentication, inferred from the phone authentication section.

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

## 30. PasswordResetToken

Password recovery token, inferred from the "Forgot Password" section.

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

---

## 31. Coupon

Discount code for orders.

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

Notes:

- `Code` is trimmed and matched case-insensitively; the unique index shall be built on the normalized code.
- `Value` must be greater than 0 for both types, and less than 100 for `Percentage`.
- `EndDate` must be later than `StartDate`, and `UsageLimit`, when set, must be greater than 0 and not lower than `UsedCount`.
- `UsedCount` is changed only through an atomic conditional update (`UsedCount < UsageLimit`) at order creation, and is released when the order becomes `Failed` or `Cancelled`.
- Orders keep their own `CouponId` and `CouponDiscountAmount` snapshot, so editing, expiring, or soft-deleting a coupon never changes existing orders.