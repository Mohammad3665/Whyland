[🇮🇷 نسخه فارسی](./Whyland-SRS-fa.md)

\# Software Requirements Specification (SRS)

\## Whyland — Educational Platform

**\*\*Project Name:\*\*** Whyland  

**\*\*Project Type:\*\*** Educational / Online Course Platform  

**\*\*Project Purpose:\*\*** Practice project and portfolio sample

**\*\*Document Version:\*\*** 1.0  

**\*\*Status:\*\*** Draft for Implementation  

**\*\*Language:\*\*** English

\---

\## 1. Purpose

This document defines the functional and non-functional requirements for an online education platform that allows users to discover, purchase, and access online courses.

The platform consists of:

1\. A public-facing website implemented with ASP.NET Core MVC.

2\. An administration panel implemented with ASP.NET Core Razor Pages.

3\. A custom authentication and authorization system without ASP.NET Core Identity.

4\. A PostgreSQL relational database.

5\. Redis for shopping cart storage and caching.

6\. An observability stack based on Serilog, OpenTelemetry, Grafana Loki, Grafana Tempo, and Prometheus.

The purpose of this SRS is to provide sufficiently precise requirements that the Whyland platform can be implemented without relying on undocumented business assumptions. Whyland is being developed as a practice project and portfolio sample to demonstrate production-oriented software architecture, backend development, web development, authorization, persistence, payments, and observability.

\---

\# 3. Project Context

Whyland is an educational platform being developed as a practice project and portfolio sample. Although the project is primarily intended for learning and demonstrating engineering skills, its architecture and requirements shall follow production-oriented practices where practical.

The implementation should demonstrate clean separation of concerns, maintainable code, realistic business rules, secure authentication and authorization, reliable order/payment processing, and complete application observability.

\# 4. Scope

\## 2.1 In Scope

The system shall provide:

\- Hierarchical course categories with a maximum depth of three levels.

\- Course creation and management.

\- Course sections and episodes.

\- Free preview episodes for paid courses.

\- Episode attachments with configurable limits.

\- Course-specific FAQs.

\- Public blog.

\- Global FAQs.

\- Dynamic users.

\- Dynamic roles.

\- Dynamic permissions.

\- Role-based permission assignment.

\- Phone-number authentication.

\- A fake SMS service for development/testing.

\- Google authentication.

\- Mandatory phone number collection before purchase.

\- User profile management.

\- Instructor profiles.

\- Course favorites.

\- Purchased-course access.

\- Redis-based shopping carts.

\- Orders and order items.

\- Payment methods represented by an enum.

\- Payment records.

\- Invoices.

\- Dynamic site settings.

\- Public MVC pages.

\- Razor Pages administration panel.

\- Permission-based authorization.

\- Structured logging.

\- Distributed tracing.

\- Metrics.

\- Grafana dashboards.

\- 403, 404, and 500 error handling.

\## 2.2 Out of Scope

The following are explicitly outside the first implementation scope:

\- Real SMS provider integration.

\- Real Google payment integration.

\- Real bank gateway integration.

\- Real ZarinPal integration.

\- Real Snapp Pay integration.

\- Instructor settlement/payout processing.

\- User-to-episode personal notes.

\- A native mobile application.

\- A separate SPA frontend.

\- ASP.NET Core Identity.

Payment providers shall only be represented by the payment-method abstraction/enum and a suitable application boundary. Real provider implementations are not required in this version.

\---

\# 4. Terminology

\| Term | Definition |

\|---|---|

\| User | An account that can authenticate and use the platform. |

\| Instructor | A user who is associated with instructor-specific profile information and may be assigned to courses. |

\| Role | A dynamically managed collection of permissions. |

\| Permission | A named authorization capability identified by a unique key. |

\| Course | A purchasable or free educational product. |

\| Section | A logical grouping of episodes within a course. |

\| Episode | An individual educational lesson within a section. |

\| Cart | A temporary collection of courses stored in Redis. |

\| Order | A persisted purchase request generated from a cart. |

\| Order Item | A course line within an order. |

\| Payment | A financial transaction associated with an order. |

\| Invoice | A financial document representing a completed order/payment. |

\| Public Website | The customer-facing MVC application. |

\| Admin Panel | The Razor Pages management application. |

\| System Role | A role protected from unrestricted deletion/modification because it is required by the application. |

\| System Permission | A permission defined by the application and protected from unrestricted deletion. |

\---

\# 5. High-Level Architecture

The application shall follow Clean Architecture.

\`\`\`text

+--------------------------+

\| Public MVC Application    |

+------------+-------------+

             |

+------------v-------------+

\| Application Layer        |

\| CQRS / MediatR           |

\| Business Rules           |

\| Result Pattern           |

+------------+-------------+

             |

+------------v-------------+

\| Domain Layer             |

\| Entities / Value Objects |

\| Domain Rules             |

+------------+-------------+

             |

+------------v-------------+

\| Infrastructure Layer     |

\| EF Core / PostgreSQL     |

\| Redis                    |

\| Authentication           |

\| SMS / External Services  |

\| Observability            |

+--------------------------+

+--------------------------+

\| Admin Razor Pages        |

+------------+-------------+

             |

             v

      Application Layer

\`\`\`

The presentation layers shall not directly implement business rules or access infrastructure concerns that belong in the Application/Infrastructure layers.

\---

\# 6. Technology Requirements

\## 5.1 .NET

The project shall use the latest supported stable .NET version available at the time implementation begins.

If .NET 11 is stable before implementation starts, the project shall use .NET 11. Otherwise, the latest appropriate stable supported version shall be used.

Preview and release-candidate versions shall not be used in production.

\## 5.2 Web Framework

\- ASP.NET Core MVC for the public website.

\- ASP.NET Core Razor Pages for the administration panel.

\## 5.3 Database

\- PostgreSQL.

\- Entity Framework Core.

\- EF Core migrations for schema evolution.

\## 5.4 Caching

\- Redis.

\- Redis shall be used for shopping carts.

\- Redis may also be used for application caching where explicitly configured.

\## 5.5 Application Architecture

The application shall use:

\- Clean Architecture.

\- CQRS.

\- MediatR.

\- Unit of Work.

\- Repository pattern.

\- Result Pattern.

\- Dependency Injection.

\---

\# 7. Repository and Unit of Work Requirements

\## 6.1 Repository Pattern

Persistence access shall be abstracted through repositories.

Repositories shall be defined around aggregate roots and meaningful domain operations rather than creating an unnecessary repository for every database table.

Examples:

\`\`\`text

ICourseRepository

ICategoryRepository

IUserRepository

IRoleRepository

IPermissionRepository

IOrderRepository

IPaymentRepository

IBlogPostRepository

\`\`\`

Repositories shall:

\- Encapsulate persistence-specific querying.

\- Return domain/application models appropriate to the layer.

\- Avoid exposing \`DbContext\` to Application handlers.

\- Avoid leaking EF Core-specific implementation details into Domain.

\- Support asynchronous operations.

\- Support cancellation tokens.

\## 6.2 Unit of Work

The persistence layer shall implement a Unit of Work abstraction.

Example:

\`\`\`csharp

public interface IUnitOfWork

{

    Task\<int> SaveChangesAsync(CancellationToken cancellationToken);

}

\`\`\`

The EF Core \`DbContext\` shall implement the Unit of Work behavior.

A single application command that changes multiple related entities shall use one Unit of Work transaction boundary where consistency is required.

Example:

\`\`\`text

Create Order

    |

    +-- Create Order

    +-- Create Order Items

    +-- Clear/consume cart

    +-- Save Changes

\`\`\`

These operations must not result in a partially persisted order.

\## 6.3 Transaction Rules

A database transaction shall be used when multiple database changes must succeed or fail together.

External service calls shall not be incorrectly included inside long-running database transactions.

Payment callbacks must be idempotent.

\## 6.4 Repository Restrictions

The following shall be avoided:

\- Generic repository solely for CRUD abstraction.

\- \`IQueryable\` exposure from repositories when it leaks persistence concerns.

\- Direct \`DbContext\` access from Controllers, Razor Pages, or MediatR handlers.

\- Business rules implemented inside EF Core configurations.

\---

\# 8. Domain Model

The core entities shall include at least:

\`\`\`text

User

UserProfile

InstructorProfile

Role

Permission

UserRole

RolePermission

Category

Course

CourseCategory

CourseInstructor

CourseSection

Episode

EpisodeAttachment

CourseFaq

FavoriteCourse

UserCourse

Cart concept in Redis

Order

OrderItem

Payment

Invoice

BlogPost

Faq

SiteSetting

\`\`\`

Audit fields such as \`CreatedAt\` and \`UpdatedAt\` shall be included where applicable.

Primary keys shall use a consistent identifier strategy throughout the system.

\---

\# 9. Category Management

\## 8.1 Fields

A Category shall contain:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Name | Required |

\| LatinName | Required |

\| DisplayOrder | Required integer |

\| ParentId | Nullable |

\| IsActive | Required boolean |

\| CreatedAt | Required |

\| UpdatedAt | Required |

\## 8.2 Hierarchy

Categories shall support a maximum of three levels.

Example:

\`\`\`text

Web Development

    └── Backend

        └── Django

\`\`\`

The following is invalid:

\`\`\`text

Web Development

    └── Backend

        └── Django

            └── Advanced Django

\`\`\`

The application shall reject creation or modification that would produce a fourth level.

\## 8.3 Rules

\- A category must have a unique identity.

\- A category may have zero or one parent.

\- A root category has \`ParentId = null\`.

\- A category cannot be its own parent.

\- Circular parent relationships are forbidden.

\- Inactive categories shall not be selectable for new course categorization.

\- Existing relationships shall not be silently removed when a category is deactivated.

\- Deleting a category that is referenced by a course shall be prevented or handled explicitly; the implementation shall not silently orphan course-category relationships.

\---

\# 10. Course Management

\## 9.1 Course Fields

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Name | Required |

\| LatinName | Required |

\| ShortDescription | Required |

\| Description | Optional/required according to validation rules; supports HTML editor content |

\| Slug | Required and unique |

\| MainImage | Optional |

\| IsFree | Required |

\| Price | Required for paid courses |

\| DiscountType | None / Percentage / FixedAmount |

\| DiscountValue | Required when discount is active |

\| IsSaleActive | Required |

\| DisplayStatus | Draft / Published / Hidden |

\| IsFeatured | Required |

\| IsAmazing | Required |

\| IntroductionVideo | Optional |

\| CreatedAt | Required |

\| UpdatedAt | Required |

\## 9.2 Description

The full description shall support HTML editor content.

The system shall sanitize HTML according to an explicit allowlist before rendering it publicly.

Raw unsanitized user-controlled HTML shall not be rendered directly.

\## 9.3 Slug

The course slug shall:

\- Be unique.

\- Be URL-safe.

\- Be stable unless explicitly changed.

\- Be used for public course URLs.

\## 9.4 Pricing

Course prices shall use a decimal-compatible monetary type.

\`float\` and \`double\` shall not be used for monetary values.

For a free course:

\`\`\`text

IsFree = true

\`\`\`

The system shall not require payment to access the course.

For a paid course:

\`\`\`text

IsFree = false

Price > 0

\`\`\`

\## 9.5 Discount

Supported discount types:

\`\`\`text

None

Percentage

FixedAmount

\`\`\`

Percentage discounts must be within:

\`\`\`text

0 <= DiscountValue <= 100

\`\`\`

Fixed discounts must not make the final price negative.

The final payable price shall be calculated centrally in the Application/Domain layer.

\## 9.6 Sale Status

\`IsSaleActive\` controls whether the course can currently be purchased.

A course may be published but not currently purchasable.

The system shall not infer sale status from display status.

\## 9.7 Display Status

Supported values:

\`\`\`text

Draft

Published

Hidden

\`\`\`

Only published courses shall be publicly discoverable as normal course listings.

Draft and hidden courses shall not be publicly exposed.

\## 9.8 Featured and Amazing

Courses may be marked:

\- Featured.

\- Amazing.

These flags correspond to existing requested special-display areas.

The system shall not require these flags to be set for a course to be published.

\---

\# 11. Course Categories

A course may belong to multiple categories.

Relationship:

\`\`\`text

Course

   |

CourseCategory

   |

Category

\`\`\`

Duplicate Course/Category relationships shall not be allowed.

Inactive categories shall not be assignable to newly created or updated courses.

\---

\# 12. Course Instructors

A course may have one or more instructors.

Relationship:

\`\`\`text

Course

   |

CourseInstructor

   |

InstructorProfile/User

\`\`\`

The system shall retain the instructor relationship independently from authentication credentials.

Only users designated as instructors through their instructor profile/role assignment shall be selectable as instructors in the Admin Panel.

\---

\# 13. Course Sections

Each course shall contain zero or more sections.

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| CourseId | Required |

\| Title | Required |

\| DisplayOrder | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Sections shall be displayed in ascending \`DisplayOrder\`.

The system may calculate and display:

\- Episode count.

\- Total video duration.

These values do not need to be persisted unless performance requirements later justify denormalization.

\---

\# 14. Episodes

Each section may contain zero or more episodes.

\## 13.1 Fields

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| SectionId | Required |

\| Title | Required |

\| Description | Optional |

\| Video | Optional/required according to episode type |

\| Duration | Required when a video exists |

\| IsFree | Required |

\| DisplayOrder | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

\## 13.2 Ordering

Episodes shall be displayed according to \`DisplayOrder\`.

Duplicate display-order values may be technically permitted, but Admin operations should maintain deterministic ordering.

\## 13.3 Free Episodes

A paid course may expose one or more episodes as free previews.

For example:

\`\`\`text

Course.IsFree = false

Episode.IsFree = true

\`\`\`

A free episode may be viewed by an unauthenticated or non-purchasing user if the course's public rules permit it.

Non-free episodes of a paid course require the user to own the course.

The access check shall be centralized in the application authorization/access layer.

\---

\# 15. Episode Attachments

An episode may have zero or more attachments.

Each attachment shall contain at least:

\`\`\`text

Id

EpisodeId

FileName

StoragePath / FileReference

ContentType

Size

CreatedAt

\`\`\`

The system shall enforce:

\- Maximum attachment count per episode.

\- Maximum individual attachment size.

\- Allowed file extensions/content types.

These limits shall be configuration-driven.

Example configuration:

\`\`\`text

MaxAttachmentsPerEpisode

MaxAttachmentSizeBytes

AllowedAttachmentExtensions

\`\`\`

The limits shall be validated server-side.

\---

\# 16. Episode Personal Notes

Personal user notes on episodes are explicitly deferred from the first implementation.

The database and application architecture should not make future implementation impossible, but no note-management feature is required in version 1.

\---

\# 17. Course FAQ

Each course may contain multiple FAQs.

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| CourseId | Required |

\| Title | Required |

\| Description | Required |

\| DisplayOrder | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

FAQs shall be displayed according to \`DisplayOrder\`.

\---

\# 18. User Management

\## 17.1 Authentication User

The authentication-oriented User entity shall contain:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| FirstName | Required |

\| LastName | Required |

\| UserName | Unique |

\| PhoneNumber | Unique where present |

\| Email | Unique where present |

\| PasswordHash | Optional for external-login-only accounts |

\| IsActive | Required |

\| LastLoginAt | Nullable |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Passwords shall never be stored in plaintext.

\## 17.2 User Profile

Authentication data shall be separated from extended profile data.

UserProfile shall contain, where applicable:

\| Field | Requirement |

\|---|---|

\| UserId | Primary/foreign key |

\| NationalCode | Optional |

\| BirthDate | Optional |

\| Gender | Optional |

\| Avatar | Optional |

\| Address | Optional |

\| Province | Optional |

\| City | Optional |

\| PostalCode | Optional |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Only information necessary for the current business requirements shall be collected.

\## 17.3 Account Activation

Inactive users shall not be allowed to authenticate or perform authenticated operations.

\---

\# 19. Instructor Profile

InstructorProfile shall extend a User with instructor-specific information.

Fields shall include:

\| Field | Requirement |

\|---|---|

\| UserId | Required |

\| ProfileImage | Optional |

\| Bio | Optional |

\| Biography | Optional |

\| Specialization | Optional |

\| Website | Optional |

\| LinkedIn | Optional |

\| Instagram | Optional |

\| NationalCode | Optional |

\| Address | Optional |

\| ShebaNumber | Optional |

\| BankAccountNumber | Optional |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Banking information is only profile information in this version. No payout or settlement workflow is included.

\---

\# 20. Authentication

The platform shall not use ASP.NET Core Identity.

Authentication shall be implemented using the application's own user, credential, role, and permission model.

\## 19.1 Phone Authentication

The system shall support phone-number-based authentication.

Basic flow:

\`\`\`text

Enter Phone Number

        |

        v

Request OTP

        |

        v

Fake SMS Service

        |

        v

Enter OTP

        |

        v

Verify OTP

        |

        v

Authenticate User

\`\`\`

The OTP shall:

\- Have a configurable expiration time.

\- Have a configurable maximum verification-attempt count.

\- Be invalidated after successful verification.

\- Not be stored in plaintext if persisted.

\- Be rate limited.

\## 19.2 Fake SMS Service

A simple interface shall be defined:

\`\`\`csharp

public interface ISmsService

{

    Task SendAsync(

        string phoneNumber,

        string message,

        CancellationToken cancellationToken);

}

\`\`\`

The development implementation shall not send a real SMS.

It may:

\- Log the OTP.

\- Store it temporarily for test purposes.

\- Return a successful delivery result.

The fake service shall be replaceable without changing Application business logic.

\---

\# 21. Google Authentication

Google OAuth/OpenID Connect authentication shall be supported.

A Google-authenticated user shall be able to create or access a local User account.

The local account shall be linked to the external Google identity through an appropriate external-login record.

Google authentication shall not replace the local User entity.

\---

\# 22. Mandatory Phone Number Before Purchase

A user may authenticate using Google without providing a phone number.

However, before creating a payable order, the system shall verify that the user's phone number is present and valid.

Flow:

\`\`\`text

Google Login

     |

     v

Add to Cart

     |

     v

Checkout

     |

     v

Phone Number Missing?

     |

   Yes

     |

     v

Collect Phone Number

     |

     v

Continue Checkout

\`\`\`

The system shall not allow payment to proceed without the required phone number.

\---

\# 23. Roles

Roles shall be fully dynamic.

The Admin Panel shall provide CRUD operations for roles.

\## 22.1 Role Fields

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Name | Required and unique |

\| Title | Required |

\| IsSystem | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

\## 22.2 Dynamic Role CRUD

Administrators with the appropriate permissions shall be able to:

\- Create roles.

\- View roles.

\- Edit roles.

\- Delete roles.

\- Assign permissions to roles.

\- Assign roles to users.

The system shall not hard-code business roles such as \`Admin\`, \`Teacher\`, or \`User\` as the only available roles.

System roles may be seeded by the application but must still be represented by the same Role entity.

\## 22.3 System Roles

\`IsSystem = true\` means the role is required by the application or protected by application rules.

The application shall prevent accidental deletion or destructive modification of system roles.

\---

\# 24. Permissions

Permissions shall be dynamically represented in the authorization model.

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Name | Required |

\| Key | Required and unique |

\| IsSystem | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Example permissions:

\`\`\`text

course.view

course.create

course.edit

course.delete

category.view

category.create

category.edit

category.delete

user.view

user.create

user.edit

user.delete

role.view

role.create

role.edit

role.delete

permission.view

permission.create

permission.edit

permission.delete

order.view

payment.view

blog.view

blog.create

blog.edit

blog.delete

setting.view

setting.edit

\`\`\`

The exact permission catalog may evolve with the Admin Panel modules.

\---

\# 25. Role-Permission Assignment

Relationship:

\`\`\`text

Role

  |

RolePermission

  |

Permission

\`\`\`

A role may have many permissions.

A permission may be assigned to many roles.

Duplicate assignments shall not be allowed.

Authorization shall evaluate the authenticated user's roles and the permissions assigned to those roles.

\---

\# 26. User-Role Assignment

Relationship:

\`\`\`text

User

  |

UserRole

  |

Role

\`\`\`

A user may have multiple roles.

Effective permissions are the union of permissions assigned through the user's roles.

If at least one assigned role grants a permission, the user is considered authorized for that permission, subject to any explicit application-level restrictions.

\---

\# 27. Permission-Based Authorization

The system shall authorize administrative operations using permission keys rather than checking role names.

Incorrect:

\`\`\`text

if user.Role == "Admin"

\`\`\`

Correct concept:

\`\`\`text

if user.HasPermission("course.create")

\`\`\`

This is required because roles are dynamic.

Authorization requirements shall be enforced server-side and shall not rely solely on UI visibility.

\---

\# 28. Favorites

Users shall be able to mark courses as favorites.

Entity:

\`\`\`text

FavoriteCourse

    UserId

    CourseId

    CreatedAt

\`\`\`

The same user shall not be able to create duplicate favorite relationships for the same course.

Users shall be able to:

\- Add a course to favorites.

\- Remove a course from favorites.

\- View their favorite courses.

\---

\# 29. Purchased Courses / Enrollment

The system shall maintain access to purchased courses.

Entity:

\`\`\`text

UserCourse

    UserId

    CourseId

    OrderId

    PurchasedAt

\`\`\`

A successful payment for a course shall grant course access.

The operation shall be idempotent.

Repeated payment callbacks or repeated processing shall not create duplicate access records.

\---

\# 30. Shopping Cart

The shopping cart shall be stored in Redis.

It shall not be persisted as the primary cart source in PostgreSQL.

\## 29.1 Cart Item

Each cart item shall contain:

\`\`\`text

CourseId

Quantity

\`\`\`

A course shall not appear more than once in a cart.

Because the product is a course entitlement, the business rule should normally limit quantity to one per course.

If the API receives a quantity greater than the allowed value, it shall reject or normalize it according to a single documented rule. The implementation shall use one consistent behavior.

\## 29.2 Cart Key

The Redis key shall be associated with the authenticated user.

Example:

\`\`\`text

cart:{userId}

\`\`\`

Unauthenticated cart persistence is not required in this version unless explicitly added later.

\---

\# 31. Orders

\## 30.1 Order

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| UserId | Required |

\| TotalAmount | Required |

\| Status | Required |

\| CreatedAt | Required |

\| PaidAt | Nullable |

\## 30.2 Order Item

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| OrderId | Required |

\| CourseId | Required |

\| Quantity | Required |

\| UnitPrice | Required |

\| TotalPrice | Required |

\## 30.3 Price Snapshot

The order shall store the effective unit price at the time the order is created.

Future changes to:

\- Course price.

\- Course discount.

shall not modify existing order amounts.

The server shall calculate all monetary totals.

Client-provided totals shall never be trusted.

\---

\# 32. Order Status

The initial status enum shall contain:

\`\`\`text

Pending

Paid

Cancelled

Failed

\`\`\`

Status transitions shall be controlled by application business rules.

An order shall not be marked \`Paid\` merely because a client reports success.

\---

\# 33. Payment Methods

Payment methods shall be represented by an enum.

Example:

\`\`\`csharp

public enum PaymentMethod

{

    BankGateway,

    ZarinPal,

    SnappPay

}

\`\`\`

No real gateway implementation is required in version 1.

The Application layer shall depend on an abstraction so that real providers can be implemented later without changing order business rules.

\---

\# 34. Payment

Payment fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| OrderId | Required |

\| PaymentMethod | Required |

\| Amount | Required |

\| Status | Required |

\| TrackingNumber | Nullable |

\| CreatedAt | Required |

\| PaidAt | Nullable |

Payment status:

\`\`\`text

Pending

Successful

Failed

Cancelled

\`\`\`

The payment amount shall correspond to the payable order amount.

\---

\# 35. Payment Idempotency

Payment result processing shall be idempotent.

If the same payment result is received multiple times, the system shall not:

\- Create duplicate payments.

\- Create duplicate enrollments.

\- Create duplicate invoices.

\- Charge the user multiple times in the application's state.

A stable payment/provider reference shall be used where applicable.

\---

\# 36. Invoice

A successful paid order shall have an invoice available from the user's profile.

Invoice information shall include:

\- Invoice number.

\- Invoice date.

\- Customer information.

\- Order reference.

\- Course/order items.

\- Quantity.

\- Unit price.

\- Discount.

\- Final amount.

\- Payment status.

The invoice must preserve the financial snapshot of the completed purchase.

\---

\# 37. Blog

\## 36.1 Blog Post Fields

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Title | Required |

\| ShortDescription | Required |

\| Content | Required |

\| AuthorId | Required |

\| Image | Optional |

\| DisplayOrder | Required |

\| DisplayStatus | Required |

\| Slug | Required and unique |

\| CreatedAt | Required |

\| UpdatedAt | Required |

The author shall reference a User.

Only content marked for public display shall appear on the public website.

\---

\# 38. General FAQ

The platform shall support global FAQs.

Fields:

\| Field | Requirement |

\|---|---|

\| Id | Unique identifier |

\| Title | Required |

\| Description | Required |

\| DisplayOrder | Required |

\| IsActive | Required |

\| CreatedAt | Required |

\| UpdatedAt | Required |

Global FAQs are separate from Course FAQs.

\---

\# 39. Site Settings

The system shall provide dynamic site settings.

Examples include:

\## General

\- Site title.

\- Logo.

\- Favicon.

\- Site description.

\## SEO

\- Default meta title.

\- Default meta description.

\- Default keywords where applicable.

\- Open Graph image.

\- Other required SEO metadata.

\## Contact

\- Phone.

\- Email.

\- Address.

\## Dynamic Content

Content that is explicitly required to be managed dynamically by the website shall be configurable through the Admin Panel.

The implementation shall avoid hard-coding editable website content into controllers or views.

\---

\# 40. Public Website

The public website shall be implemented with ASP.NET Core MVC.

\## 39.1 Home Page

The home page shall support existing requested course/content areas, including:

\- Featured courses.

\- Amazing courses.

\- Other course sections required by the configured site content.

\- Recent articles.

\- Global FAQ where configured.

\## 39.2 Course Listing

The course listing page shall display published courses.

The page shall support category-aware course discovery.

At minimum it shall display:

\- Course title.

\- Main image.

\- Short description.

\- Price.

\- Discount/final price where applicable.

\- Free/paid state.

\## 39.3 Course Details

The course details page shall display:

\- Course title.

\- Main image.

\- Short description.

\- Full description.

\- Categories.

\- Instructor information.

\- Price.

\- Discount.

\- Introduction video.

\- Sections.

\- Episodes.

\- Episode durations.

\- Free episodes.

\- Course FAQs.

\## 39.4 Blog Listing

The blog listing page shall display public blog posts.

\## 39.5 Blog Details

The article page shall display:

\- Title.

\- Short description.

\- Content.

\- Author.

\- Image.

\- Publication/display state.

\## 39.6 Authentication Pages

Login and registration may be combined into a unified authentication flow.

The authentication experience shall support:

\- Phone OTP.

\- Google login.

\- Password recovery where a local password exists.

\## 39.7 Forgot Password

The system shall provide a password-recovery flow.

The recovery mechanism shall use a secure, time-limited token or equivalent mechanism.

Password-reset tokens shall be:

\- Time limited.

\- Single use.

\- Invalidated after successful reset.

\---

\# 41. User Profile

The profile area shall contain:

\## 40.1 User Information

Display the user's current profile and account information.

\## 40.2 Edit Information

Users may update allowed profile fields.

Sensitive identity fields such as National Code may require additional validation or may be made non-editable after verification if such verification is introduced.

\## 40.3 Change Password

Authenticated users with a local password shall be able to change their password.

\## 40.4 Favorite Courses

Display the user's favorite courses.

\## 40.5 Purchased Courses

Display courses for which the user has access.

\## 40.6 Payment History

Display the user's payment history.

\## 40.7 Invoices

Users shall be able to view/download their invoices where supported by the presentation layer.

\---

\# 42. Cart Page

The cart page shall display:

\- Course.

\- Quantity.

\- Unit price.

\- Discount.

\- Final line total.

\- Cart total.

Users shall be able to remove items.

Quantity modification shall respect course-purchase rules.

\---

\# 43. Checkout

Checkout shall contain exactly three conceptual stages.

\`\`\`text

1\. Information

        |

        v

2\. Payment

        |

        v

3\. Payment Result

\`\`\`

\## 42.1 Stage 1 — Information

The system shall validate required customer information.

The phone number is mandatory before payment.

\## 42.2 Stage 2 — Payment

The user shall select one of the supported payment-method enum values.

The application shall create the necessary pending payment/order state.

\## 42.3 Stage 3 — Payment Result

The system shall display:

\- Successful payment.

\- Failed payment.

For successful payment:

\`\`\`text

Payment Successful

        |

        v

Order = Paid

        |

        v

Grant Course Access

        |

        v

Create/Finalize Invoice

\`\`\`

The complete flow shall be transactionally consistent where database operations are involved.

\---

\# 44. Administration Panel

The administration panel shall be implemented using Razor Pages.

All administrative operations shall be permission protected.

\## 43.1 Required Admin Modules

The panel shall provide management for:

\- Categories.

\- Courses.

\- Course sections.

\- Episodes.

\- Episode attachments.

\- Course FAQs.

\- Users.

\- User profiles.

\- Instructors.

\- Roles.

\- Permissions.

\- Role-permission assignments.

\- User-role assignments.

\- Orders.

\- Payments.

\- Blog posts.

\- Global FAQs.

\- Site settings.

\---

\# 45. CRUD Requirements

Where an entity is defined as administratively manageable, the Admin Panel shall provide appropriate:

\- Create.

\- Read/list.

\- Update.

\- Delete.

operations.

CRUD operations shall respect:

\- Validation.

\- Authorization.

\- Referential integrity.

\- System-entity restrictions.

\- Audit timestamps.

Delete operations that could destroy important historical/financial data shall use an appropriate business rule rather than blindly issuing a hard delete.

For example, paid orders and successful payments shall not be physically deleted through ordinary Admin CRUD.

\---

\# 46. Error Handling

The public website and administration panel shall support:

\## 45.1 HTTP 400

For invalid client/application input where appropriate.

\## 45.2 HTTP 403

Displayed when an authenticated user lacks the required permission.

\## 45.3 HTTP 404

Displayed when a requested resource does not exist or is not publicly accessible.

\## 45.4 HTTP 500

Displayed for unexpected server-side errors.

Internal exception details, stack traces, database errors, and secrets shall never be displayed to end users.

All unexpected errors shall be logged with appropriate correlation information.

\---

\# 47. Observability

The system shall provide:

\- Logs.

\- Distributed traces.

\- Metrics.

The target stack is:

\`\`\`text

Application

    |

    +-- Serilog

    |

    +-- OpenTelemetry

           |

           +-- Logs   -> Loki

           +-- Traces -> Tempo

           +-- Metrics -> Prometheus

                            |

                            v

                         Grafana

\`\`\`

Grafana shall provide unified dashboards for operational and business observability.

\---

\# 48. Logging Requirements

Logs shall be structured.

At minimum, relevant request logs shall contain:

\- Timestamp.

\- Level.

\- Message.

\- Exception when present.

\- Trace ID.

\- Span ID when available.

\- User ID when available.

\- HTTP method.

\- Request path.

\- Status code.

\- Duration.

Sensitive values shall not be logged.

The system shall not log:

\- Passwords.

\- OTP values in production.

\- Authentication tokens.

\- Payment secrets.

\- Full sensitive personal information.

The Fake SMS service may log OTPs only in development/test environments.

\---

\# 49. Distributed Tracing Requirements

OpenTelemetry shall instrument relevant application operations.

Traces should cover:

\- Incoming HTTP requests.

\- Database operations.

\- Redis operations.

\- External service calls.

\- Authentication operations.

\- Payment operations.

Trace identifiers shall be correlated with application logs.

\---

\# 50. Metrics Requirements

The system shall expose application and business metrics.

\## 49.1 Default/System Metrics

At minimum:

\- Request count.

\- Request duration.

\- Active requests where supported.

\- HTTP status distribution.

\- Error rate.

\- Process/runtime metrics.

\- Database performance indicators.

\- Redis performance indicators where available.

\## 49.2 Business Metrics

The system shall expose:

\- Course views.

\- Purchases.

\- Successful payments.

\- Failed payments.

\- Registration count.

\- Conversion rate where calculable.

\## 49.3 Slow Operation Metrics

The system shall make slow operations observable, including:

\- Slow HTTP requests.

\- Slow database queries.

\- Slow external service calls.

Thresholds shall be configuration-driven.

\---

\# 51. Grafana Dashboards

At minimum, the project shall provide dashboards for:

\## Application

\- Request rate.

\- Response time.

\- Error rate.

\- HTTP 4xx.

\- HTTP 5xx.

\## Business

\- Course views.

\- Purchases.

\- Successful payments.

\- Failed payments.

\- Conversion rate.

\## Logs

\- Error logs.

\- Warning logs.

\- Application logs.

\## Traces

\- Slow requests.

\- Failed requests.

\- Trace details.

Prometheus shall act as the metrics backend, Loki as the log backend, and Tempo as the trace backend.

\---

\# 52. Security Requirements

\## 51.1 Password Security

Passwords shall be securely hashed using a modern password hashing mechanism.

Plaintext passwords shall never be stored.

\## 51.2 Authentication Security

Authentication tokens/cookies shall:

\- Be protected against tampering.

\- Use secure cookie settings in production.

\- Have appropriate expiration.

\- Support logout/invalidation according to the selected authentication mechanism.

\## 51.3 Authorization

All protected operations shall be authorized server-side.

Hiding an Admin UI button shall never be considered sufficient authorization.

\## 51.4 Input Validation

All external input shall be validated.

This includes:

\- Form input.

\- Query parameters.

\- Route parameters.

\- File uploads.

\- Payment results.

\- OAuth responses.

\## 51.5 HTML Content

HTML editor content shall be sanitized.

\## 51.6 File Uploads

Attachments shall be validated by:

\- Extension.

\- Content type where possible.

\- Size.

\- Count.

\- Storage rules.

Uploaded files shall not automatically become executable content.

\---

\# 53. Data Integrity

The database shall enforce appropriate constraints, including:

\- Required fields.

\- Unique fields.

\- Foreign keys.

\- Unique composite relationships.

\- Valid enum representations where appropriate.

Important business invariants shall be enforced in the Application/Domain layer and not rely solely on UI validation.

\---

\# 54. Concurrency

Operations that can be triggered simultaneously shall be designed to avoid inconsistent state.

Important examples:

\- Two payment callbacks for the same transaction.

\- Two checkout requests for the same cart.

\- Duplicate favorite requests.

\- Concurrent role-permission assignment.

\- Concurrent order processing.

Database unique constraints and application-level idempotency shall be used together where necessary.

\---

\# 55. Caching

Redis shall be used for:

\- Shopping carts.

\- OTP temporary state where appropriate.

\- Explicitly configured application cache entries.

Cached data shall have an expiration policy.

Critical persisted business data shall not exist only in Redis.

\---

\# 56. Configuration

The following values shall be configuration-driven:

\- Database connection.

\- Redis connection.

\- Authentication settings.

\- Google OAuth settings.

\- OTP expiration.

\- OTP attempt limits.

\- SMS fake-service behavior.

\- Attachment limits.

\- Allowed attachment extensions.

\- Slow request threshold.

\- Slow query threshold.

\- Observability endpoints.

\- Payment environment settings.

Secrets shall be supplied through secure environment/configuration mechanisms and shall not be committed to source control.

\---

\# 57. Database Migration

Entity Framework Core migrations shall be used.

Database schema changes shall be reproducible from source control.

Production database migration strategy shall not rely on manually editing the database.

\---

\# 58. API/Application Boundary

Even though the public website and Admin Panel are MVC/Razor Pages applications, business operations shall be implemented in the Application layer.

Examples:

\`\`\`text

CreateCourseCommand

UpdateCourseCommand

DeleteCourseCommand

CreateCategoryCommand

AssignCourseCategoryCommand

CreateRoleCommand

UpdateRoleCommand

AssignPermissionToRoleCommand

CreateOrderCommand

CreatePaymentCommand

ProcessPaymentResultCommand

AddFavoriteCourseCommand

RemoveFavoriteCourseCommand

\`\`\`

Queries shall be separated from commands according to CQRS.

\---

\# 59. Result Pattern

Application operations shall use a Result Pattern rather than using exceptions as the normal mechanism for expected business failures.

Expected failures may include:

\- Course not found.

\- Category hierarchy limit exceeded.

\- User inactive.

\- Permission denied.

\- Phone number required.

\- Invalid discount.

\- Course unavailable for sale.

\- Insufficient course access.

\- Invalid payment state.

Unexpected infrastructure/system failures shall still use exception handling and centralized error handling.

\---

\# 60. CQRS and MediatR

Commands shall modify state.

Queries shall retrieve data without modifying business state.

MediatR shall be used as the application dispatch mechanism for commands and queries where appropriate.

Cross-cutting concerns such as:

\- Validation.

\- Logging.

\- Performance measurement.

\- Transaction handling where appropriate.

may be implemented through MediatR pipeline behaviors.

\---

\# 61. Validation

Validation shall exist at multiple levels:

\### Presentation

Basic input validation for user experience.

\### Application

Business validation and command validation.

\### Domain

Invariants that must always hold.

\### Database

Final integrity constraints.

No single layer shall be treated as the only source of validation.

\---

\# 62. Public Access Rules

The public website shall only expose content according to its state.

Examples:

\`\`\`text

Course:

    Published -> public

    Draft     -> not public

    Hidden    -> not public

Blog:

    Published/display-enabled -> public

    Otherwise                 -> not public

\`\`\`

Paid course content shall require course ownership except for explicitly free episodes.

\---

\# 63. Course Access Rules

A user may access a course if:

\`\`\`text

Course.IsFree = true

\`\`\`

or:

\`\`\`text

User owns the course

\`\`\`

or, for a paid course:

\`\`\`text

Episode.IsFree = true

\`\`\`

Free preview access shall not grant ownership of the complete course.

\---

\# 64. Purchase Integrity

Before order creation, the application shall re-evaluate:

\- Course existence.

\- Course publication state where applicable.

\- Sale availability.

\- Current price.

\- Current discount.

\- User eligibility.

\- Phone-number requirement.

\- Existing ownership.

The server shall never trust price or discount values received from the browser.

\---

\# 65. Existing Ownership

If a user already owns a course, the system shall not create a second course entitlement for the same user/course pair.

The cart/checkout flow should prevent unnecessary repurchase of already-owned courses.

\---

\# 66. Auditability

At minimum, entities should retain:

\`\`\`text

CreatedAt

UpdatedAt

\`\`\`

Financial and authorization operations should produce structured logs sufficient to trace:

\- Who performed the operation.

\- What operation occurred.

\- When it occurred.

\- What request/trace initiated it.

Sensitive data must not be logged.

\---

\# 67. Suggested Project Structure

\`\`\`text

src/

│

├── Project.Domain/

│   ├── Entities/

│   ├── Enums/

│   ├── ValueObjects/

│   ├── Exceptions/

│   └── Common/

│

├── Project.Application/

│   ├── Features/

│   │   ├── Categories/

│   │   ├── Courses/

│   │   ├── Users/

│   │   ├── Roles/

│   │   ├── Permissions/

│   │   ├── Orders/

│   │   ├── Payments/

│   │   ├── Blog/

│   │   └── Settings/

│   ├── Common/

│   ├── Behaviors/

│   └── Interfaces/

│

├── Project.Infrastructure/

│   ├── Persistence/

│   │   ├── Context/

│   │   ├── Configurations/

│   │   ├── Repositories/

│   │   └── Migrations/

│   ├── Redis/

│   ├── Authentication/

│   ├── Sms/

│   ├── Payments/

│   └── Observability/

│

├── Project.Web/

│   ├── Controllers/

│   ├── Views/

│   ├── ViewModels/

│   └── Middleware/

│

└── Project.Admin/

    ├── Pages/

    ├── ViewModels/

    └── Middleware/

\`\`\`

\---

\# 68. Required Initial System Data

The application shall seed the minimum data required for first operation.

This may include:

\- Initial system roles.

\- Initial system permissions.

\- Initial role-permission assignments.

\- Required system settings.

Seeded roles remain dynamic database records and must not be replaced by hard-coded role-name authorization.

\---

\# 69. Non-Functional Requirements

\## 68.1 Performance

The application shall:

\- Use asynchronous I/O.

\- Avoid blocking calls.

\- Use pagination for potentially large admin/public lists.

\- Avoid N+1 database queries.

\- Monitor slow database queries.

\- Monitor slow HTTP requests.

\## 68.2 Scalability

The application should remain stateless at the web-server layer as much as practical.

Session/cart state shall not depend on local server memory.

Redis shall be used for shared temporary state.

\## 68.3 Maintainability

The codebase shall:

\- Follow Clean Architecture boundaries.

\- Separate Domain, Application, Infrastructure, and Presentation concerns.

\- Use dependency injection.

\- Use automated tests for important business rules.

\- Avoid duplicated business logic.

\## 68.4 Reliability

Critical operations shall be idempotent where repeated requests are possible.

Payment processing is specifically required to be idempotent.

\## 68.5 Security

The application shall follow secure defaults for:

\- Authentication.

\- Authorization.

\- Password storage.

\- File uploads.

\- HTML rendering.

\- Secrets.

\- Logging.

\- Input validation.

\---

\# 70. Testing Requirements

The project should contain:

\## Unit Tests

For:

\- Pricing.

\- Discount calculations.

\- Category hierarchy validation.

\- Course-access rules.

\- Order calculations.

\- Permission evaluation.

\- Payment-state transitions.

\## Integration Tests

For:

\- PostgreSQL persistence.

\- Repository behavior.

\- Unit of Work transactions.

\- Redis cart operations.

\- Authentication flows.

\- Payment-result processing.

\## End-to-End Tests

For critical flows:

\`\`\`text

Registration/Login

       ↓

Browse Course

       ↓

Add to Cart

       ↓

Checkout

       ↓

Payment

       ↓

Course Access

       ↓

Invoice

\`\`\`

\---

\# 71. Acceptance Criteria — Core Flows

\## 70.1 Course Purchase

The following must be true:

1\. A user can browse published courses.

2\. A paid course can be added to the Redis cart.

3\. Checkout requires a phone number.

4\. The server calculates the order price.

5\. An order is created.

6\. A payment record is created.

7\. A successful payment changes the order to \`Paid\`.

8\. The user receives course access.

9\. An invoice becomes available.

10\. Reprocessing the same payment result does not duplicate access or invoice state.

\## 70.2 Dynamic Roles

The following must be true:

1\. An authorized administrator can create a role.

2\. The administrator can edit the role.

3\. The administrator can assign permissions.

4\. The administrator can assign the role to users.

5\. The user receives the role's permissions.

6\. Authorization is based on permission keys.

7\. The administrator can remove the role when it is not protected as a system role.

\## 70.3 Course Structure

The following must be true:

1\. A course can contain multiple sections.

2\. A section can contain multiple episodes.

3\. Sections are ordered.

4\. Episodes are ordered.

5\. Episode duration is available.

6\. A paid course can contain multiple free episodes.

7\. Non-free episodes require course ownership.

\---

\# 72. Explicit Business Rules Summary

The following rules are mandatory:

1\. Category hierarchy cannot exceed three levels.

2\. Course slugs are unique.

3\. Blog slugs are unique.

4\. Phone numbers are unique when present.

5\. Emails are unique when present.

6\. Passwords are never stored in plaintext.

7\. Roles are dynamic.

8\. Permissions are dynamic.

9\. Roles receive permissions.

10\. Authorization uses permissions rather than hard-coded role names.

11\. System roles/permissions are protected from destructive changes.

12\. Shopping carts are stored in Redis.

13\. Order prices are snapshotted.

14\. Payment processing is idempotent.

15\. Successful payment grants course access.

16\. Free episodes of paid courses can be viewed without purchasing the complete course.

17\. Paid non-free episodes require course ownership.

18\. Users authenticated through Google must provide a phone number before payment.

19\. Real SMS provider integration is not required.

20\. Real payment-provider integration is not required.

21\. Course attachments have count and size limits.

22\. Attachment restrictions are configuration-driven.

23\. Public content must respect its display status.

24\. Admin operations must be permission-protected.

25\. Unexpected server errors must be logged and must not expose sensitive details.

26\. Logs, traces, and metrics must be available through the observability stack.

27\. Grafana must provide unified observability dashboards.

28\. Repository and Unit of Work abstractions must be used for persistence access.

29\. Controllers and Razor Pages must not directly access EF Core \`DbContext\`.

30\. Business logic belongs in the Application/Domain layers.

\---

\# 73. Final Technology Stack

\| Area | Technology |

\|---|---|

\| Runtime | Latest supported stable .NET |

\| Public Website | ASP.NET Core MVC |

\| Admin Panel | ASP.NET Core Razor Pages |

\| ORM | Entity Framework Core |

\| Database | PostgreSQL |

\| Cache | Redis |

\| Architecture | Clean Architecture |

\| CQRS | MediatR |

\| Persistence | Repository + Unit of Work |

\| Result Handling | Result Pattern |

\| Authentication | Custom authentication |

\| Authorization | Permission-based |

\| External Login | Google |

\| SMS | Fake SMS service |

\| Logging | Serilog |

\| Telemetry | OpenTelemetry |

\| Logs Backend | Grafana Loki |

\| Tracing Backend | Grafana Tempo |

\| Metrics Backend | Prometheus |

\| Dashboards | Grafana |

\---

\# 74. Implementation Principles

The implementation shall follow these principles:

\- Do not use ASP.NET Core Identity.

\- Do not hard-code roles as the authorization mechanism.

\- Do not hard-code permissions in presentation logic.

\- Do not store shopping carts as the primary cart state in PostgreSQL.

\- Do not trust client-side prices.

\- Do not store plaintext passwords.

\- Do not expose internal exceptions to users.

\- Do not expose unsanitized HTML.

\- Do not allow category depth beyond three levels.

\- Do not duplicate business rules between MVC, Razor Pages, and Application services.

\- Do not expose \`DbContext\` outside Infrastructure.

\- Do not use a generic repository solely for the sake of having a repository.

\- Use Unit of Work for consistent persistence boundaries.

\- Make payment-result processing idempotent.

\- Keep external integrations behind abstractions.

\- Keep configuration and secrets outside source code.

\- Prefer explicit domain/application rules over implicit behavior.

\---

\# 75. Definition of Done

A feature is considered complete when:

1\. Its Domain model is implemented where required.

2\. Its Application command/query is implemented.

3\. Validation is implemented.

4\. Authorization is implemented where required.

5\. Persistence is implemented through the repository/unit-of-work boundary.

6\. Required database constraints exist.

7\. The Public Website or Admin Panel UI is implemented as applicable.

8\. Error handling is implemented.

9\. Relevant logs and telemetry exist.

10\. Automated tests cover important business rules.

11\. The feature does not bypass architectural boundaries.

12\. No sensitive information is exposed through logs or UI.

13\. The feature follows the business rules defined in this document.

\---

\# 76. Conclusion

This SRS defines the first implementation scope of the online course platform.

The system is intentionally designed around:

\`\`\`text

Clean Architecture

        \+

CQRS / MediatR

        \+

Repository / Unit of Work

        \+

Result Pattern

        \+

PostgreSQL

        \+

Redis

        \+

Custom Permission-Based Authorization

        \+

Serilog / OpenTelemetry

        \+

Loki / Tempo / Prometheus

        \+

Grafana

\`\`\`

The system must remain extensible without introducing unnecessary features outside the defined scope. Dynamic roles and permissions are a core requirement, while payment and SMS providers are intentionally abstracted so real integrations can be added later without changing the core business model.

# 77. Data Deletion Strategy

The application shall use **Soft Delete by default**.

Physical deletion shall not be used for normal application entities unless there is a specific technical or business requirement that explicitly requires permanent deletion.

## 77.1 Soft Delete Fields

Entities that support deletion shall contain:

| Field     | Requirement        |
| --------- | ------------------ |
| IsDeleted | Required boolean   |
| DeletedAt | Nullable timestamp |
| DeletedBy | Nullable User ID   |

Where appropriate, entities shall also contain:

| Field     | Requirement                                             |
| --------- | ------------------------------------------------------- |
| CreatedBy | Nullable/required User ID according to entity lifecycle |
| UpdatedBy | Nullable/required User ID according to entity lifecycle |

The audit fields shall normally be:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
IsDeleted
DeletedAt
DeletedBy
```

## 77.2 Default Query Behavior

Soft-deleted entities shall not be returned by normal application queries.

The Infrastructure layer should implement this behavior through EF Core global query filters where appropriate.

A query that explicitly needs deleted records must opt into that behavior explicitly.

## 77.3 Permanent Deletion

Hard deletion is permitted only for entities where permanent deletion is explicitly justified.

Examples may include:

* Temporary technical records.
* Disposable development/test records.
* Data that is explicitly required to be permanently removed by a documented business/privacy rule.

Financial records such as successful payments, paid orders, and issued invoices shall not be physically deleted through ordinary administrative operations.

---

# 78. Audit Fields

The application shall maintain creation and modification audit information.

At minimum, business entities that require auditability shall contain:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

Where an operation represents deletion:

```text
DeletedAt
DeletedBy
IsDeleted
```

## 78.1 CreatedBy

`CreatedBy` shall reference the User responsible for creating the record where the operation is performed by an authenticated user.

For system-generated records, a documented system identity or nullable value may be used.

## 78.2 UpdatedBy

`UpdatedBy` shall reference the User responsible for the latest modification.

## 78.3 DeletedBy

`DeletedBy` shall reference the User who performed the soft-delete operation.

The application shall not trust client-provided values for these fields.

Audit information shall be populated by the server/Application/Infrastructure layer.

---

# 79. Repository Architecture

The repository architecture shall explicitly separate **read** and **write** responsibilities.

A single generic repository shall not be used.

The application shall use explicit repositories designed around actual business requirements.

## 79.1 Write Repositories

Write repositories shall be responsible for operations that modify application state.

Examples:

```text
ICourseWriteRepository
ICategoryWriteRepository
IUserWriteRepository
IRoleWriteRepository
IPermissionWriteRepository
IOrderWriteRepository
IPaymentWriteRepository
IInvoiceWriteRepository
```

Write repositories shall use **Entity Framework Core**.

EF Core shall be responsible for:

* Inserts.
* Updates.
* Soft deletes.
* Relationship changes.
* Change tracking where required.
* Transactional persistence.

## 79.2 Read Repositories

Read repositories shall be responsible for retrieving data.

Examples:

```text
ICourseReadRepository
ICategoryReadRepository
IUserReadRepository
IRoleReadRepository
IPermissionReadRepository
IOrderReadRepository
IPaymentReadRepository
IInvoiceReadRepository
```

Read repositories shall use **Dapper**.

Dapper shall be preferred for read-oriented queries where direct SQL projection provides simpler or more efficient data retrieval.

Read repositories should return application-specific DTOs/read models rather than exposing EF Core entities unnecessarily.

## 79.3 No Generic Repository

The application shall not implement a generic repository such as:

```csharp
IGenericRepository<T>
```

solely to provide CRUD operations.

Repositories shall be explicit and use-case-oriented.

For example:

```csharp
public interface ICourseReadRepository
{
    Task<CourseDetailsDto?> GetDetailsAsync(
        Guid courseId,
        CancellationToken cancellationToken);
}
```

and:

```csharp
public interface ICourseWriteRepository
{
    Task AddAsync(
        Course course,
        CancellationToken cancellationToken);

    Task<Course?> GetForUpdateAsync(
        Guid courseId,
        CancellationToken cancellationToken);
}
```

The repository abstraction shall represent meaningful application operations rather than merely wrapping database APIs.

---

# 80. Read/Write Persistence Rules

The persistence implementation shall follow these rules:

```text
                    Application
                         |
              +----------+----------+
              |                     |
              v                     v
       Read Repositories     Write Repositories
              |                     |
              v                     v
           Dapper                EF Core
              |                     |
              +----------+----------+
                         |
                     PostgreSQL
```

## 80.1 Read Operations

Queries shall:

* Use Dapper through read repositories.
* Prefer explicit SQL projections.
* Return DTOs/read models.
* Avoid loading complete aggregate graphs when only a projection is required.
* Support pagination for large result sets.

## 80.2 Write Operations

Commands shall:

* Use EF Core through write repositories.
* Apply business rules in the Application/Domain layer.
* Use Unit of Work where multiple changes must be persisted consistently.
* Use transactions where atomicity is required.

## 80.3 DbContext Boundary

`DbContext` shall remain an Infrastructure implementation detail.

Controllers, Razor Pages, and Application handlers shall not directly access `DbContext`.

---

# 81. Product Pricing Model

Where the domain contains a `Product` concept, the pricing model shall be changed.

The system shall not persist a separate `IsFree` database column.

Instead:

```text
Price = null
```

shall represent a free product.

A non-null price represents a paid product.

## 81.1 Product Fields

The model shall use:

```csharp
public decimal? Price { get; private set; }
```

instead of requiring:

```csharp
public bool IsFree { get; set; }
```

## 81.2 IsFree Property

The domain model may expose:

```csharp
public bool IsFree => Price is null;
```

`IsFree` is a derived domain property and shall not be persisted in the database.

Conceptually:

```text
Price = null
    => IsFree = true

Price > 0
    => IsFree = false
```

## 81.3 Pricing Rules

A product with:

```text
Price = null
```

is free.

A product with:

```text
Price > 0
```

is paid.

Zero-priced paid products shall not be used to represent free products unless a separate documented business requirement explicitly introduces such a state.

## 81.4 Course Application

Because `Course` currently represents the purchasable educational product, the same pricing rule shall apply to Course unless a separate Product aggregate is introduced.

Therefore, the previous requirement:

```text
IsFree = Required
Price = Required for paid courses
```

shall be replaced by:

```text
Price = Nullable
IsFree = Derived property and not persisted
```

The access rule becomes:

```text
Course.Price == null
    => Course is free

Course.Price != null
    => Course is paid
```

The same rule shall be applied to episode-level free-preview behavior where applicable.

---

# 82. Cookie-Based Authentication

Authentication shall be **cookie-based**.

The application shall not use JWT bearer tokens as the primary browser authentication mechanism.

## 82.1 Authentication Mechanism

After successful authentication, the server shall issue an authentication cookie.

The browser shall automatically send the authentication cookie with subsequent requests.

Conceptually:

```text
Login / OTP Verification
        |
        v
Server validates identity
        |
        v
Authentication Cookie
        |
        v
Authenticated Browser Requests
```

## 82.2 Cookie Security

Authentication cookies shall use secure production settings, including where applicable:

```text
HttpOnly = true
Secure = true
SameSite = appropriate restrictive value
```

The exact SameSite behavior shall be selected according to the application's authentication and external-login requirements.

## 82.3 Authentication State

The authenticated user identity shall be represented by claims inside the authentication cookie.

The cookie shall not contain sensitive information unnecessarily.

Passwords, OTPs, payment secrets, and other sensitive values shall never be stored inside authentication cookies.

## 82.4 Logout

Logout shall invalidate the authenticated session/cookie according to the configured cookie authentication mechanism.

## 82.5 Google Authentication

Google authentication shall still use the appropriate external OAuth/OpenID Connect flow.

After successful external authentication, the application shall establish its own local cookie-based authenticated session.

Google access/identity tokens shall not be used as the application's normal browser authentication mechanism.

---

# 83. SMS Gateway Simulation Service

The fake SMS functionality shall be implemented as a **separate API project** alongside the main Whyland application.

It shall simulate the behavior of a real SMS provider API.

The purpose is to allow the main application to communicate with an SMS API through a realistic HTTP boundary without requiring a real SMS provider.

## 83.1 Project Structure

The solution shall contain a separate project, for example:

```text
src/
│
├── Project.Domain/
├── Project.Application/
├── Project.Infrastructure/
├── Project.Web/
├── Project.Admin/
│
└── Project.Sms/
```

`Project.Sms` shall be an independent ASP.NET Core Web API application.

It shall not contain Whyland business logic.

## 83.2 SMS API

The API shall expose endpoints similar to the interface normally provided by an SMS panel/provider.

At minimum:

```text
POST /api/v1/sms/send
```

The request shall contain information equivalent to:

```json
{
  "to": "09120000000",
  "message": "Your verification code is 123456"
}
```

The response shall contain a provider-like result:

```json
{
  "success": true,
  "messageId": "..."
}
```

The exact request/response contract shall be documented and versioned.

## 83.3 Provider-Oriented Design

The Whyland application shall communicate with an abstraction such as:

```csharp
public interface ISmsService
{
    Task<SendSmsResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken);
}
```

The Infrastructure implementation shall call the separate SMS API over HTTP.

Therefore:

```text
Whyland Application
        |
        v
ISmsService
        |
        v
HTTP Client
        |
        v
Project.Sms API
        |
        v
In-Memory Storage
```

This keeps the main application independent from the fake provider implementation.

## 83.4 In-Memory Database

The SMS API shall use an in-memory data store for the first implementation.

It shall store at least:

```text
SMS Requests
SMS Messages
Recipient
Message
Status
MessageId
CreatedAt
SentAt
```

Persistence to PostgreSQL is not required for the SMS simulation service.

The service shall be restartable without requiring database migrations.

## 83.5 SMS Dashboard

The SMS service shall provide a simple HTML-based administration/dashboard page.

The dashboard shall allow developers to easily inspect:

* Incoming SMS API requests.
* Recipient phone numbers.
* Message contents.
* Generated message IDs.
* SMS status.
* Request timestamps.
* Sent timestamps.

Example:

```text
SMS Simulator
-------------------------------------------------
Message ID | Recipient | Message | Status | Date
-------------------------------------------------
12345      | 0912...   | OTP ... | Sent   | ...
12346      | 0913...   | OTP ... | Sent   | ...
-------------------------------------------------
```

The dashboard is intended for development/testing and does not need to be a production-grade administration system.

## 83.6 SMS Dashboard Requirements

The dashboard shall support:

* Listing SMS requests.
* Viewing an individual SMS request.
* Basic filtering by recipient/status.
* Displaying request time.
* Displaying message status.
* Displaying message ID.

The dashboard may use Razor Pages or MVC.

## 83.7 Real Provider Replacement

The fake SMS service shall be replaceable by a real provider implementation without changing Application-layer business logic.

Example future implementations:

```text
FakeSmsService
KavenegarSmsService
OtherProviderSmsService
```

The provider-specific HTTP contract shall remain outside the Domain layer.

---

# 84. SMS API Documentation and Compatibility

The SMS simulator API shall be designed after reviewing the public API model of an existing SMS provider.

The implementation should follow common provider concepts such as:

```text
API Key / Authentication
Recipient
Sender
Message
Message ID
Provider Status
Request Status
```

The simulator shall not unnecessarily copy a provider's private implementation.

Its purpose is to reproduce the integration pattern expected by a real SMS provider.

The selected provider documentation shall be kept as an external reference for implementation.

---

# 85. Invoice Generation and Reporting

The invoice generation implementation shall be evaluated separately from the financial domain model.

The Invoice domain entity shall represent the financial snapshot.

A reporting/PDF library shall be responsible for rendering that snapshot into a document.

The system shall not put PDF/reporting concerns directly into the Domain layer.

Architecture:

```text
Order / Payment
      |
      v
Invoice Domain Model
      |
      v
Invoice Application Service
      |
      v
Invoice Report Generator
      |
      v
PDF / Printable Invoice
```

## 85.1 Candidate Libraries

The following .NET reporting/PDF technologies were reviewed:

### Stimulsoft Reports

Stimulsoft Reports.WEB supports ASP.NET, ASP.NET Core, MVC, Razor Pages and browser-based report design/viewing. Its documentation also explicitly covers invoice reports and exporting reports to PDF.

Stimulsoft also provides an official .NET package and examples showing report loading, rendering, and exporting to PDF.

It can also expose rendered reports through a web API endpoint, which is relevant if invoice generation eventually needs to be provided as a dedicated service.

Stimulsoft additionally documents support for electronic-invoice-related formats such as ZUGFeRD and Factur-X.

### QuestPDF

QuestPDF is a C# PDF-generation library with a component-based layout system.

Its official documentation provides a dedicated invoice tutorial and recommends separating invoice/document models from the rendering logic.

It can generate PDFs directly to files, byte arrays, or streams.

### FastReport .NET

FastReport .NET provides reporting functionality and PDF export capabilities. Its official documentation includes a PDF export API and comprehensive .NET reporting documentation.

## 85.2 Selection Criteria

The invoice/reporting solution shall be evaluated against:

| Criterion                 | Requirement                            |
| ------------------------- | -------------------------------------- |
| .NET compatibility        | Must support the selected .NET version |
| ASP.NET Core integration  | Required                               |
| PDF output                | Required                               |
| Invoice templates         | Required                               |
| Persian/RTL support       | Required                               |
| Custom fonts              | Required                               |
| Table/layout support      | Required                               |
| Template maintainability  | Required                               |
| Browser preview           | Preferred                              |
| Printing                  | Preferred                              |
| Excel/Word export         | Optional                               |
| API/server-side rendering | Preferred                              |
| Barcode/QR support        | Preferred                              |
| E-invoice support         | Preferred where relevant               |
| Testability               | Required                               |
| Deployment complexity     | Should be low                          |
| Licensing                 | Must be reviewed before final adoption |

## 85.3 Recommended Evaluation

The current preferred candidate is:

**Stimulsoft Reports.WEB**

The reason is that the project already has an ASP.NET Core web architecture and the product specifically targets ASP.NET/ASP.NET Core reporting, report design, browser viewing, PDF export, and invoice-style documents.

However, the final library selection shall be made after reviewing:

* Licensing cost.
* Persian/RTL invoice rendering.
* Required fonts.
* Deployment requirements.
* Report template workflow.
* PDF output quality.
* Required export formats.
* Long-term maintenance.
* Commercial usage requirements.

QuestPDF shall remain a strong alternative when the team prefers a code-first PDF generation model rather than a visual report designer. Its official invoice tutorial makes it particularly relevant to this use case.

## 85.4 Invoice Rendering Boundary

The application shall define an abstraction such as:

```csharp
public interface IInvoiceRenderer
{
    Task<byte[]> RenderPdfAsync(
        InvoiceDocument document,
        CancellationToken cancellationToken);
}
```

The Domain layer shall not reference Stimulsoft, QuestPDF, FastReport, or any other reporting library.

The Infrastructure layer shall contain the concrete implementation.

For example:

```text
IInvoiceRenderer
       |
       +---- StimulsoftInvoiceRenderer
```

The implementation can later be replaced without modifying the Invoice domain model.

---

# 86. Invoice Data Model

The invoice shall preserve the financial snapshot at the time it is issued.

The invoice shall not calculate historical prices by querying the current Course entity.

An invoice shall contain its own immutable financial information.

At minimum:

```text
Invoice Number
Invoice Date
Customer
Customer Phone
Customer Email
Order Reference
Payment Reference
Items
Quantity
Unit Price
Discount
Tax where applicable
Subtotal
Final Amount
Payment Status
```

Each invoice item shall contain its own price snapshot.

For example:

```text
InvoiceItem
    Product/Course Name
    Quantity
    UnitPrice
    Discount
    TotalPrice
```

Changes to the Course price after invoice issuance shall not change the invoice.

---

# 87. Invoice Immutability

Once an invoice has been finalized/issued, its financial contents shall not be modified through ordinary CRUD operations.

Corrections shall be handled through an explicit documented process if such a process is introduced later.

The system shall not silently overwrite historical invoice information when:

* Course prices change.
* User information changes.
* Discounts change.
* Product names change.
* Payment information changes.

---

# 88. Updated Repository and Architecture Summary

The final persistence architecture shall therefore be:

```text
                 Presentation
              MVC / Razor Pages
                       |
                       v
                 Application
              CQRS / MediatR
                       |
              +--------+--------+
              |                 |
              v                 v
       Read Repositories   Write Repositories
              |                 |
            Dapper            EF Core
              |                 |
              +--------+--------+
                       |
                   PostgreSQL
```

Redis remains responsible for temporary/shared application state such as:

```text
Shopping Cart
OTP Temporary State
Configured Cache Entries
```

The SMS simulator is an independent application:

```text
Project.Sms
     |
 In-Memory Store
     |
 HTML Dashboard
```

---

# 89. Updated Authentication Architecture

The final authentication flow shall be:

```text
                    +----------------+
                    | Phone / Google |
                    +-------+--------+
                            |
                            v
                    Local User Account
                            |
                            v
                  Cookie Authentication
                            |
                            v
                    Authenticated Request
                            |
                            v
                 Permission-Based Authorization
```

JWT bearer authentication shall not be used as the primary authentication mechanism for the browser application.

---

# 90. Updated Course Pricing Rules

The previous Course pricing requirements shall be replaced by:

```text
Course.Price : decimal?
```

and:

```csharp
public bool IsFree => Price is null;
```

`IsFree` shall not be persisted.

Therefore:

```text
Price == null
    => Free Course

Price != null
    => Paid Course
```

The application shall use this rule consistently in:

* Course listing.
* Course details.
* Cart.
* Checkout.
* Order creation.
* Course access.
* Invoice generation.
* Admin Panel.

---

# 91. Updated Audit Requirements

The following fields shall be considered standard audit fields:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
IsDeleted
DeletedAt
DeletedBy
```

Not every field is necessarily mandatory for every entity, but all persistent business entities shall explicitly document which audit fields apply.

For entities participating in authorization, financial operations, content management, and administrative operations, auditability shall be treated as mandatory.

---

# 92. Updated Explicit Business Rules Summary

The following rules are mandatory and supersede conflicting previous requirements:

1. Soft delete is the default deletion strategy.

2. Hard delete is permitted only for explicitly documented exceptions.

3. Soft-deletable entities shall contain `IsDeleted`, `DeletedAt`, and `DeletedBy` where applicable.

4. Business entities shall contain `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy` where applicable.

5. Normal queries shall exclude soft-deleted records.

6. Read and write repositories shall be separated.

7. Read repositories shall use Dapper.

8. Write repositories shall use Entity Framework Core.

9. A generic repository shall not be implemented.

10. `DbContext` shall remain inside Infrastructure.

11. Controllers, Razor Pages, and Application handlers shall not directly access `DbContext`.

12. Course/Product free status shall be derived from a nullable `Price`.

13. `IsFree` shall not be persisted in the database.

14. Browser authentication shall be cookie-based.

15. JWT shall not be the primary browser authentication mechanism.

16. Google authentication shall establish a local cookie-based authenticated session.

17. The fake SMS provider shall be a separate API project.

18. The SMS API shall use an in-memory data store.

19. The SMS API shall expose a provider-like HTTP API.

20. The SMS API shall provide an HTML dashboard for inspecting SMS requests.

21. The fake SMS service shall be replaceable by a real provider implementation.

22. Invoice rendering shall be isolated behind an abstraction.

23. The Domain layer shall not reference reporting/PDF libraries.

24. Stimulsoft Reports.WEB is the current preferred invoice/reporting candidate.

25. QuestPDF and FastReport .NET shall remain documented alternatives.

26. Invoice data shall preserve historical financial snapshots.

27. Finalized invoices shall not be modified through ordinary CRUD operations.

28. Payment processing shall remain idempotent.

29. Financial records shall not be physically deleted through normal administration.

30. All sensitive information shall remain excluded from logs and authentication cookies.

---

# 93. Revised Technology Stack

| Area                    | Technology                                      |
| ----------------------- | ----------------------------------------------- |
| Runtime                 | Latest supported stable .NET                    |
| Public Website          | ASP.NET Core MVC                                |
| Admin Panel             | ASP.NET Core Razor Pages                        |
| ORM / Write Persistence | Entity Framework Core                           |
| Read Persistence        | Dapper                                          |
| Database                | PostgreSQL                                      |
| Cache / Temporary State | Redis                                           |
| Architecture            | Clean Architecture                              |
| CQRS                    | MediatR                                         |
| Persistence             | Explicit Read/Write Repositories + Unit of Work |
| Result Handling         | Result Pattern                                  |
| Authentication          | Cookie-based custom authentication              |
| Authorization           | Permission-based                                |
| External Login          | Google                                          |
| SMS                     | Separate Fake SMS API                           |
| SMS Storage             | In-Memory                                       |
| SMS Dashboard           | ASP.NET Core HTML UI                            |
| Invoice Reporting       | Stimulsoft Reports.WEB — preferred              |
| Invoice Alternative     | QuestPDF                                        |
| Reporting Alternative   | FastReport .NET                                 |
| Logging                 | Serilog                                         |
| Telemetry               | OpenTelemetry                                   |
| Logs Backend            | Grafana Loki                                    |
| Tracing Backend         | Grafana Tempo                                   |
| Metrics Backend         | Prometheus                                      |
| Dashboards              | Grafana                                         |

---

# 94. Updated Implementation Principles

The implementation shall additionally follow these principles:

* Prefer soft delete over physical deletion.
* Preserve audit information for administrative changes.
* Separate read and write persistence paths.
* Use Dapper for read-oriented repositories.
* Use EF Core for write-oriented repositories.
* Do not introduce a generic repository.
* Do not persist a redundant `IsFree` field.
* Derive free status from nullable `Price`.
* Use secure cookie-based authentication for the browser.
* Keep the SMS simulator outside the main application process.
* Treat the SMS simulator as an external HTTP dependency.
* Store SMS simulator state in memory.
* Provide a simple HTML dashboard for SMS inspection.
* Keep invoice rendering independent from the Domain layer.
* Preserve immutable financial snapshots in invoices.
* Keep provider-specific implementation details behind abstractions.
* Keep reporting-library dependencies outside Domain and Application business models where possible.
* Do not physically delete historical financial records.
* Do not allow audit fields to be supplied by the client.

---

## 95. Coupon Business Rules
 
1. Coupon validity shall be re-checked server-side at order creation time, not only when the code is first applied, to prevent stale-state or race-condition exploitation.
2. `UsedCount` shall be incremented atomically to prevent over-redemption under concurrent requests.
3. `Code` uniqueness shall be enforced at the persistence layer.
4. Discount calculation logic (percentage vs. fixed amount) shall reside in the Domain layer and shall not be duplicated in the Application or presentation layers.
5. A per-user redemption limit is not modeled directly on the Coupon entity. If required, it shall be enforced through a separate redemption-tracking record linking Coupon, User, and Order.
6. Coupon fields shall follow the standard audit and soft-deletion requirements defined in sections 78 and 77.

---
 
# 96. Revised Definition of Done
 
A feature is considered complete when:
 
1. Domain requirements are implemented.
2. Application commands/queries are implemented.
3. Read operations use the read-repository boundary.
4. Write operations use the write-repository boundary.
5. Read repositories use Dapper.
6. Write repositories use EF Core.
7. No generic repository has been introduced.
8. Required audit fields are populated.
9. Soft deletion is implemented where applicable.
10. Authentication uses the cookie-based mechanism.
11. Authorization is permission-based.
12. Required persistence constraints exist.
13. Required UI is implemented.
14. SMS integration uses the external SMS API boundary where applicable.
15. Invoice rendering is isolated behind an abstraction.
16. Financial snapshots are preserved.
17. Error handling is implemented.
18. Relevant logs and telemetry exist.
19. Automated tests cover important business rules.
20. No architectural boundary is bypassed.
21. No sensitive information is exposed through logs, cookies, or UI.
22. The implementation follows all mandatory business rules defined in this SRS.
