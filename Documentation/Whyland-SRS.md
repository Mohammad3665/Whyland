# Software Requirements Specification (SRS)

## Whyland — Educational Platform

> [🇮🇷 نسخه فارسی SRS](./Whyland-SRS-fa.md) · This is the English version.

- [1. Introduction & Project Scope](#1-introduction--project-scope)
  - [1.1 Purpose](#11-purpose)
  - [1.2 Project Context](#12-project-context)
  - [1.3 Project Scope](#13-project-scope)
  - [1.4 Terminology](#14-terminology)
- [2. Architecture & Technology](#2-architecture--technology)
  - [2.1 High-Level Architecture](#21-high-level-architecture)
  - [2.2 Technology Requirements](#22-technology-requirements)
  - [2.3 Technology Stack](#23-technology-stack)
  - [2.4 Suggested Project Structure](#24-suggested-project-structure)
  - [2.5 API/Application Boundary](#25-apiapplication-boundary)
  - [2.6 CQRS and MediatR](#26-cqrs-and-mediatr)
  - [2.7 Result Pattern](#27-result-pattern)
  - [2.8 Validation](#28-validation)
  - [2.9 Configuration](#29-configuration)
- [3. Domain Model & Persistence](#3-domain-model--persistence)
  - [3.1 Domain Model](#31-domain-model)
  - [3.2 Repository and Unit of Work Requirements](#32-repository-and-unit-of-work-requirements)
  - [3.3 Repository Architecture](#33-repository-architecture)
  - [3.4 Read/Write Persistence Rules](#34-readwrite-persistence-rules)
  - [3.5 Data Integrity](#35-data-integrity)
  - [3.6 Concurrency](#36-concurrency)
  - [3.7 Caching](#37-caching)
  - [3.8 Database Migration](#38-database-migration)
- [4. Data Deletion & Audit](#4-data-deletion--audit)
  - [4.1 Data Deletion Strategy](#41-data-deletion-strategy)
  - [4.2 Audit Fields](#42-audit-fields)
  - [4.3 Auditability](#43-auditability)
- [5. Users & Authentication](#5-users--authentication)
  - [5.1 User Management](#51-user-management)
  - [5.2 Instructor Profile](#52-instructor-profile)
  - [5.3 Authentication](#53-authentication)
  - [5.4 Google Authentication](#54-google-authentication)
  - [5.5 Mandatory Phone Number Before Purchase](#55-mandatory-phone-number-before-purchase)
  - [5.6 Cookie-Based Authentication](#56-cookie-based-authentication)
- [6. Roles, Permissions & Access Rules](#6-roles-permissions--access-rules)
  - [6.1 Roles](#61-roles)
  - [6.2 Permissions](#62-permissions)
  - [6.3 Permission–Role Assignment](#63-permissionrole-assignment)
  - [6.4 Role–User Assignment](#64-roleuser-assignment)
  - [6.5 Permission-Based Authorization](#65-permission-based-authorization)
  - [6.6 Required Initial System Data](#66-required-initial-system-data)
  - [6.7 Public Access Rules](#67-public-access-rules)
  - [6.8 Course Access Rules](#68-course-access-rules)
- [7. Educational Catalog (Categories, Courses, Episodes)](#7-educational-catalog-categories-courses-episodes)
  - [7.1 Category Management](#71-category-management)
  - [7.2 Course Management](#72-course-management)
  - [7.3 Product Pricing Model](#73-product-pricing-model)
  - [7.4 Course Categories](#74-course-categories)
  - [7.5 Course Instructors](#75-course-instructors)
  - [7.6 Course Sections](#76-course-sections)
  - [7.7 Episodes](#77-episodes)
  - [7.8 Episode Attachments](#78-episode-attachments)
  - [7.9 Episode Personal Notes](#79-episode-personal-notes)
  - [7.10 Course FAQ](#710-course-faq)
  - [7.11 Favorites](#711-favorites)
- [8. Video & File Storage (wwwroot)](#8-video--file-storage-wwwroot)
  - [8.1 Video Storage & Serving](#81-video-storage--serving)
  - [8.2 Course Downloads](#82-course-downloads)
  - [8.3 Upload Security & File Processing](#83-upload-security--file-processing)
  - [8.4 HTTP, Browser & Content Serving Security](#84-http-browser--content-serving-security)
  - [8.5 Access Control & Data Integrity](#85-access-control--data-integrity)
  - [8.6 Security Test Requirements](#86-security-test-requirements)
  - [8.7 Feature Completion Criteria](#87-feature-completion-criteria)
- [9. Cart, Orders & Payments](#9-cart-orders--payments)
  - [9.1 Purchased Courses / Enrollment](#91-purchased-courses--enrollment)
  - [9.2 Existing Ownership](#92-existing-ownership)
  - [9.3 Shopping Cart](#93-shopping-cart)
  - [9.4 Coupon Business Rules](#94-coupon-business-rules)
  - [9.5 Orders](#95-orders)
  - [9.6 Order Status](#96-order-status)
  - [9.7 Purchase Integrity](#97-purchase-integrity)
  - [9.8 Payment Methods](#98-payment-methods)
  - [9.9 Payment](#99-payment)
  - [9.10 Payment Idempotency](#910-payment-idempotency)
- [10. Invoice](#10-invoice)
  - [10.1 Invoice](#101-invoice)
  - [10.2 Invoice Data Model](#102-invoice-data-model)
  - [10.3 Invoice Immutability](#103-invoice-immutability)
  - [10.4 Invoice Generation & Reporting](#104-invoice-generation--reporting)
- [11. SMS Service](#11-sms-service)
  - [11.1 Fake SMS Gateway Service](#111-fake-sms-gateway-service)
  - [11.2 SMS API Documentation & Compatibility](#112-sms-api-documentation--compatibility)
  - [11.3 SMS Provider Layer & SMS Template Management](#113-sms-provider-layer--sms-template-management)
- [12. Public Content (Blog, FAQ, Site Settings)](#12-public-content-blog-faq-site-settings)
  - [12.1 Blog](#121-blog)
  - [12.2 Public FAQ](#122-public-faq)
  - [12.3 Site Settings](#123-site-settings)
- [13. UI: Public Website & Admin Panel](#13-ui-public-website--admin-panel)
  - [13.1 Public Website](#131-public-website)
  - [13.2 User Profile](#132-user-profile)
  - [13.3 Cart Page](#133-cart-page)
  - [13.4 Checkout Process](#134-checkout-process)
  - [13.5 Admin Panel](#135-admin-panel)
  - [13.6 CRUD Requirements](#136-crud-requirements)
  - [13.7 Error Handling](#137-error-handling)
- [14. Observability](#14-observability)
  - [14.1 Observability](#141-observability)
  - [14.2 Logging Requirements](#142-logging-requirements)
  - [14.3 Distributed Tracing Requirements](#143-distributed-tracing-requirements)
  - [14.4 Metrics Requirements](#144-metrics-requirements)
  - [14.5 Grafana Dashboards](#145-grafana-dashboards)
- [15. Security & Non-Functional Requirements](#15-security--non-functional-requirements)
  - [15.1 Security Requirements](#151-security-requirements)
  - [15.2 Non-Functional Requirements](#152-non-functional-requirements)
- [16. Testing, Acceptance Criteria & Definition of Done](#16-testing-acceptance-criteria--definition-of-done)
  - [16.1 Testing Requirements](#161-testing-requirements)
  - [16.2 Acceptance Criteria — Core Flows](#162-acceptance-criteria--core-flows)
  - [16.3 Definition of Done](#163-definition-of-done)
- [17. Rules, Key Principles & Conclusion](#17-rules-key-principles--conclusion)
  - [17.1 Explicit Business Rules Summary](#171-explicit-business-rules-summary)
  - [17.2 Implementation Principles](#172-implementation-principles)
  - [17.3 Conclusion](#173-conclusion)

## 1. Introduction & Project Scope

### 1.1 Purpose

This document defines the functional and non-functional requirements of an online education platform that allows users to discover, purchase, and access online courses.

The platform includes the following:

1. A public website implemented with ASP.NET Core MVC.
2. An admin panel implemented with ASP.NET Core Razor Pages.
3. A custom authentication and authorization system without using ASP.NET Core Identity.
4. A PostgreSQL relational database.
5. Redis for cart storage and caching.
6. Storage of videos and downloadable course files in the application's `wwwroot` folder.
7. An observability infrastructure based on Serilog, OpenTelemetry, Grafana Loki, Grafana Tempo, and Prometheus.

The purpose of this SRS is to provide requirements with enough precision so that the Whyland platform can be implemented without relying on undocumented business assumptions. Whyland is developed as a practice/portfolio project to demonstrate production-oriented software architecture, backend development, web development, authorization, persistence, payments, and observability.

### 1.2 Project Context

Whyland is an educational platform developed as a practice and portfolio project. Although the primary goal of this project is learning and demonstrating engineering skills, its architecture and requirements must follow production-oriented practices as much as possible.

The implementation must demonstrate a clear separation of concerns, maintainable code, realistic business rules, secure authentication and authorization, reliable order/payment processing, and full application observability.

### 1.3 Project Scope

#### 1.3.1 In Scope

The system must provide the following:

- Hierarchical course categories with a maximum depth of three levels.
- Course creation and management.
- Course sections and episodes.
- Free preview episodes for paid courses.
- Episode attachments with configurable limits.
- Protected storage and playback of MP4 videos and downloadable course files from the `wwwroot` folder.
- Course-specific FAQs.
- A public blog.
- Public FAQs.
- Dynamic users.
- Dynamic roles.
- Dynamic permissions.
- Role-based permission assignment.
- Phone number authentication.
- A fake SMS service for development/testing.
- Google authentication.
- Mandatory phone number collection before purchase.
- User profile management.
- Instructor profiles.
- Course favorites.
- Access to purchased courses.
- Redis-backed shopping carts.
- Orders and order items.
- Payment methods represented by an enum.
- Payment records.
- Invoices.
- Discount coupons applicable in the cart and checkout flow.
- Dynamic site settings.
- Public MVC pages.
- Razor Pages admin panel.
- Permission-based authorization.
- Structured logging.
- Distributed tracing.
- Metrics.
- Grafana dashboards.
- Handling of 403, 404, and 500 errors.

#### 1.3.2 Out of Scope

The following are explicitly out of scope for the first implementation:

- Integration with a real SMS provider.
- Real Google Pay integration.
- Real bank gateway integration.
- Real ZarinPal integration.
- Real SnappPay integration.
- Instructor payout/settlement processing.
- User personal notes on episodes.
- A native mobile application.
- A separate SPA frontend.
- ASP.NET Core Identity.

Payment providers must be represented only through the payment-method abstraction/enum and a suitable application-level boundary. Implementing real providers is not required in this version.

### 1.4 Terminology

| Term              | Definition                                                                              |
| ----------------- | --------------------------------------------------------------------------------------- |
| User              | An account that can authenticate and use the platform.                                  |
| Instructor        | A user associated with dedicated instructor profile information, assignable to courses. |
| Role              | A dynamic, manageable collection of permissions.                                        |
| Permission        | A named authorization capability identified by a unique key.                            |
| Course            | A purchasable or free educational product.                                              |
| Section           | A logical grouping of episodes within a course.                                         |
| Episode           | An individual educational lesson within a section.                                      |
| Cart              | A temporary collection of courses stored in Redis.                                      |
| Order             | A persisted purchase request generated from a cart.                                     |
| Order Item        | A course line item within an order.                                                     |
| Payment           | A financial transaction associated with an order.                                       |
| Invoice           | A financial document representing a completed order/payment.                            |
| Public Website    | The customer-facing MVC application.                                                    |
| Admin Panel       | The Razor Pages management application.                                                 |
| System Role       | A role required by the application and protected against unrestricted deletion/changes. |
| System Permission | A permission defined by the application and protected against unrestricted deletion.    |

## 2. Architecture & Technology

### 2.1 High-Level Architecture

The application must follow Clean Architecture.

```text
+--------------------------+
| Public MVC Application   |
+------------+-------------+
             |
+------------v-------------+
| Application Layer        |
| CQRS / MediatR           |
| Business Rules           |
| Result Pattern           |
+------------+-------------+
             |
+------------v-------------+
| Domain Layer             |
| Entities / Value Objects |
| Domain Rules             |
+------------+-------------+
             |
+------------v-------------+
| Infrastructure Layer     |
| EF Core / PostgreSQL     |
| Redis                    |
| File Storage (wwwroot)   |
| Authentication           |
| SMS / External Services  |
| Observability            |
+--------------------------+

+--------------------------+
| Admin Razor Pages        |
+------------+-------------+
             |
             v
      Application Layer
```

Presentation layers must not implement business rules directly or access Infrastructure concerns that belong to the Application/Infrastructure layers.

### 2.2 Technology Requirements

#### 2.2.1 .NET

The project must use the latest stable, supported .NET version available at the start of implementation.

If .NET 11 becomes stable before implementation starts, the project must use .NET 11. Otherwise, the latest suitable stable, supported version must be used.

Preview and Release Candidate versions must not be used in production.

#### 2.2.2 Web Framework

- ASP.NET Core MVC for the public website.
- ASP.NET Core Razor Pages for the admin panel.

#### 2.2.3 Database

- PostgreSQL.
- Entity Framework Core.
- EF Core migrations for schema evolution.

#### 2.2.4 Caching

- Redis.
- Redis must be used for shopping carts.
- Redis may also be used for application caching where explicitly configured.

#### 2.2.5 File Storage

- Course MP4 video files and protected downloadable records are stored in the application's `wwwroot` folder.
- Protected files must be served through permission-checked, controlled ASP.NET Core endpoints; direct static access to protected files must not be possible.
- Its architecture, access control, and security are defined in Chapter 8.

#### 2.2.6 Application Architecture

The application must use:

- Clean Architecture.
- CQRS.
- MediatR.
- Unit of Work.
- The Repository pattern.
- The Result Pattern.
- Dependency Injection (DI).

### 2.3 Technology Stack

| Area                    | Technology                                                           |
| ----------------------- | -------------------------------------------------------------------- |
| Runtime                 | Latest stable, supported .NET version                                |
| Public website          | ASP.NET Core MVC                                                     |
| Admin panel             | ASP.NET Core Razor Pages                                             |
| ORM / write persistence | Entity Framework Core                                                |
| Read persistence        | Dapper                                                               |
| Database                | PostgreSQL                                                           |
| Cache / temporary state | Redis                                                                |
| File/video storage      | The application's `wwwroot` folder (MP4 serving with access control) |
| Architecture            | Clean Architecture                                                   |
| CQRS                    | MediatR                                                              |
| Persistence             | Explicit read/write repositories + Unit of Work                      |
| Result handling         | Result Pattern                                                       |
| Authentication          | Custom cookie-based authentication                                   |
| Authorization           | Permission-based                                                     |
| External login          | Google                                                               |
| SMS                     | Standalone fake SMS API                                              |
| SMS API client          | Refit (standalone Project.Sms.Provider package)                      |
| SMS storage             | In-Memory for SMS records; durable database for templates (s11.3.5)  |
| SMS dashboard           | ASP.NET Core HTML-based UI                                           |
| Invoice reporting       | Stimulsoft Reports.WEB — preferred                                   |
| Invoice alternative     | QuestPDF                                                             |
| Reporting alternative   | FastReport .NET                                                      |
| Logging                 | Serilog                                                              |
| Telemetry               | OpenTelemetry                                                        |
| Log backend             | Grafana Loki                                                         |
| Tracing backend         | Grafana Tempo                                                        |
| Metrics backend         | Prometheus                                                           |
| Dashboard               | Grafana                                                              |

### 2.4 Suggested Project Structure

```text
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
│   │   ├── Coupons/
│   │   ├── Invoices/
│   │   ├── Blog/
│   │   ├── Settings/
│   │   └── Media/
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
│   ├── Storage/
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
├── Project.Admin/
│   ├── Pages/
│   ├── ViewModels/
│   └── Middleware/
│
├── Project.Sms/
└── Project.Sms.Provider/
```

### 2.5 API/Application Boundary

Although the public website and admin panel are MVC/Razor Pages applications, business operations must be implemented in the Application layer.

Examples:

```text
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
```

Queries must be separated from Commands according to CQRS.

### 2.6 CQRS and MediatR

Commands must change state.

Queries must retrieve data without changing business state.

MediatR must be used as the application's dispatch mechanism for Commands and Queries, where applicable.

Cross-cutting concerns such as:

- Validation.
- Logging.
- Performance measurement.
- Transaction management, where applicable.

can be implemented through MediatR pipeline behaviors.

### 2.7 Result Pattern

Application operations must use a Result Pattern instead of using exceptions as the normal mechanism for expected business errors.

Expected errors can include:

- Course not found.
- Category hierarchy constraint rejected.
- Inactive user.
- Permission denied.
- Phone number required.
- Invalid discount.
- Course not available for sale.
- Insufficient course access.
- Invalid payment status.

Unexpected infrastructure/system errors must still use exception handling and centralized error management.

### 2.8 Validation

Validation must exist at multiple levels:

#### 2.8.1 Presentation

Basic input validation for user experience.

#### 2.8.2 Application

Business validation and command validation.

#### 2.8.3 Domain

Invariants that must always hold.

#### 2.8.4 Database

Final integrity constraints.

No layer alone should be considered the single source of validation.

### 2.9 Configuration

The following values must be configuration-driven:

- Database connection.
- Redis connection.
- Authentication settings.
- Google OAuth settings.
- OTP expiry.
- OTP attempt limits.
- Fake SMS service behavior.
- Attachment limits.
- Allowed attachment extensions.
- Slow request threshold.
- Slow query threshold.
- Observability endpoints.
- Payment environment settings.
- Pending-order expiration duration.
- Coupon application rate limits.

Secrets must be provided through secure environment/configuration mechanisms and must not be committed to source control.

## 3. Domain Model & Persistence

### 3.1 Domain Model

The core entities must include at least the following:

```text
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
```

Audit fields such as `CreatedAt` and `UpdatedAt` must be included where applicable.

Primary keys must use a single consistent ID strategy across the system.

### 3.2 Repository and Unit of Work Requirements

#### 3.2.1 Repository Pattern

Persistence access must be abstracted through repositories.

Repositories must be defined around Aggregate Roots and meaningful domain operations, not by creating one unnecessary repository per database table.

Examples:

```text
ICourseWriteRepository / ICourseReadRepository
ICategoryWriteRepository / ICategoryReadRepository
IUserWriteRepository / IUserReadRepository
IRoleWriteRepository / IRoleReadRepository
IPermissionWriteRepository / IPermissionReadRepository
IOrderWriteRepository / IOrderReadRepository
IPaymentWriteRepository / IPaymentReadRepository
IBlogPostWriteRepository / IBlogPostReadRepository
```

The read/write repository separation is defined in s3.3.

Repositories must:

- Encapsulate dedicated persistence queries.
- Return layer-appropriate Domain/Application models.
- Avoid exposing `DbContext` to Application handlers.
- Avoid leaking EF Core-specific implementation details into the Domain.
- Support asynchronous operations.
- Support Cancellation Tokens.

#### 3.2.2 Unit of Work

The persistence layer must implement a Unit of Work abstraction.

Example:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

The EF Core `DbContext` must implement the Unit of Work behavior.

A single application command that modifies several related entities must use a single Unit-of-Work transactional boundary where consistency is required.

Example:

```text
Create order
    |
    +-- Create order
    +-- Create order items
    +-- Clear/consume shopping cart
    +-- Save changes
```

These operations must not result in a partially saved order.

#### 3.2.3 Transaction Rules

Where multiple database changes must succeed or fail together, a database transaction must be used.

External service calls must not accidentally be placed inside long-running database transactions.

Payment callbacks must be idempotent.

#### 3.2.4 Repository Restrictions

The following must be avoided:

- Generic repositories serving only as CRUD abstraction.
- Exposing `IQueryable` from repositories where it leaks persistence concerns.
- Direct `DbContext` access from Controllers, Razor Pages, or MediatR handlers.
- Implementing business rules inside EF Core configurations.

### 3.3 Repository Architecture

The repository architecture must explicitly separate **Read** and **Write** responsibilities.

A single generic repository must not be used.

The application must use explicit repositories designed around real business needs.

#### 3.3.1 Write Repositories

Write repositories are responsible for operations that change application state.

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

Write repositories must use **Entity Framework Core**.

EF Core must be responsible for:

- Insert.
- Update.
- Soft Delete.
- Relationship changes.
- Change tracking where required.
- Transactional persistence.

#### 3.3.2 Read Repositories

Read repositories are responsible for data retrieval.

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

Read repositories must use **Dapper**.

Dapper should be preferred for read-oriented queries where a direct SQL projection is simpler or more efficient.

Read repositories must return dedicated application DTO/read models instead of unnecessarily exposing EF Core entities.

#### 3.3.3 No Generic Repository

The application must not implement a generic repository such as the following just to provide CRUD operations:

```csharp
IGenericRepository<T>
```

Repositories must be explicit and use-case based.

For example:

```csharp
public interface ICourseReadRepository
{
    Task<CourseDetailsDto?> GetDetailsAsync(
        Guid courseId,
        CancellationToken cancellationToken);
}
```

And:

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

The repository abstraction must represent meaningful application operations, not merely a wrapper over database APIs.

### 3.4 Read/Write Persistence Rules

The persistence implementation must follow the rules below. The final persistence architecture is:

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

#### 3.4.1 Read Operations

Queries must:

- Use Dapper through read repositories.
- Prefer explicit SQL projections.
- Return DTO/read models.
- Avoid fully loading aggregate graphs where only a projection is needed.
- Support pagination for large result sets.

#### 3.4.2 Write Operations

Commands must:

- Use EF Core through write repositories.
- Enforce business rules in the Application/Domain layer.
- Use Unit of Work where multiple changes must be persisted consistently.
- Use transactions where atomicity is required.

#### 3.4.3 DbContext Boundary

The `DbContext` must remain an Infrastructure implementation detail.

Controllers, Razor Pages, and Application handlers must not access the `DbContext` directly.

### 3.5 Data Integrity

The database must enforce appropriate constraints, including:

- Required fields.
- Unique fields.
- Foreign keys.
- Unique composite relations.
- Valid enum representations where applicable.

Important business invariants must be enforced in the Application/Domain layer and must not rely solely on UI validation.

### 3.6 Concurrency

Operations that can run concurrently must be designed to prevent inconsistent state.

Important examples:

- Two payment callbacks for the same transaction.
- Two checkout requests for the same cart.
- Duplicate favorite requests.
- Concurrent permission assignment to a role.
- Concurrent order processing.
- Concurrent coupon usage.
- Pending-order expiration racing with a payment result.

Database unique constraints and application-level idempotency must be used together where required.

### 3.7 Caching

Redis must be used for:

- Shopping carts.
- Temporary OTP state when needed.
- Explicitly configured application cache entries.

Cached data must have an expiration policy.

Persisted critical business data must not exist only in Redis.

### 3.8 Database Migration

Entity Framework Core migrations must be used.

Database schema changes must be reproducible from source control.

The production database migration strategy must not rely on manual database editing.

## 4. Data Deletion & Audit

### 4.1 Data Deletion Strategy

The application must use **Soft Delete** by default.

Physical deletion must not be used for ordinary application entities unless a specific technical or business requirement explicitly mandates permanent deletion.

#### 4.1.1 Soft Delete Fields

Entities that support deletion must include:

| Field     | Requirement          |
| --------- | -------------------- |
| IsDeleted | Required boolean     |
| DeletedAt | Timestamp (nullable) |
| DeletedBy | User ID (nullable)   |

Where appropriate, entities should also include:

| Field     | Requirement                                                  |
| --------- | ------------------------------------------------------------ |
| CreatedBy | User ID, nullable/required depending on the entity lifecycle |
| UpdatedBy | User ID, nullable/required depending on the entity lifecycle |

Audit fields should typically include:

```text
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
IsDeleted
DeletedAt
DeletedBy
```

#### 4.1.2 Default Query Behavior

Soft-deleted entities must not be returned by ordinary application queries.

The Infrastructure layer must implement this behavior where appropriate via EF Core Global Query Filters.

A query that explicitly needs deleted records must explicitly opt in to this behavior.

#### 4.1.3 Permanent Deletion

Physical (hard) deletion is allowed only for entities whose permanent removal is explicitly justified.

Examples can include:

- Temporary technical records.
- Consumed development/test records.
- Data that must be permanently deleted per a documented business/privacy rule.

Financial records such as successful payments, paid orders, and issued invoices must not be physically deleted through ordinary administrative operations.

### 4.2 Audit Fields

The application must maintain creation and modification audit information.

At a minimum, business entities that require auditability must include:

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

#### 4.2.1 CreatedBy

`CreatedBy` must reference the user responsible for creating the record, where the operation is performed by an authenticated user.

For system-generated records, either a documented system identity or a nullable value can be used.

#### 4.2.2 UpdatedBy

`UpdatedBy` must reference the user responsible for the latest change.

#### 4.2.3 DeletedBy

`DeletedBy` must reference the user who performed the soft-delete operation.

The application must not trust client-supplied values for these fields.

Audit information must be populated by the server/Application/Infrastructure layer.

#### 4.2.4 Field Applicability

Not every field is necessarily required for every entity, but all persisted business entities must explicitly specify which audit fields apply to them.

For entities participating in authorization, financial operations, content management, and administrative operations, auditability must be considered mandatory.

## 5. Users & Authentication

### 5.1 User Management

#### 5.1.1 Authentication User

The authentication-centric User entity must include the following:

| Field        | Requirement                               |
| ------------ | ----------------------------------------- |
| Id           | Unique identifier                         |
| FirstName    | Required                                  |
| LastName     | Required                                  |
| UserName     | Unique                                    |
| PhoneNumber  | Unique when present                       |
| Email        | Unique when present                       |
| PasswordHash | Optional for external-login-only accounts |
| IsActive     | Required                                  |
| LastLoginAt  | Nullable                                  |
| CreatedAt    | Required                                  |
| UpdatedAt    | Required                                  |

Passwords must never be stored in plaintext.

#### 5.1.2 User Profile

Authentication data must be separated from extended profile data.

UserProfile must include, where applicable:

| Field        | Requirement |
| ------------ | ----------- |
| UserId       | PK/FK       |
| BirthDate    | Optional    |
| Gender       | Optional    |
| Avatar       | Optional    |
| CreatedAt    | Required    |
| UpdatedAt    | Required    |

Only information strictly necessary for the current business requirements must be collected.

#### 5.1.3 Account Activation

Inactive users must not be allowed to authenticate or perform operations that require authentication.

### 5.2 Instructor Profile

InstructorProfile must extend a User with dedicated instructor information.

Fields must include:

| Field             | Requirement |
| ----------------- | ----------- |
| UserId            | Required    |
| ProfileImage      | Optional    |
| Bio               | Optional    |
| Biography         | Optional    |
| Specialization    | Optional    |
| Website           | Optional    |
| LinkedIn          | Optional    |
| Instagram         | Optional    |
| NationalCode      | Optional    |
| Address           | Optional    |
| ShebaNumber       | Optional    |
| BankAccountNumber | Optional    |
| CreatedAt         | Required    |
| UpdatedAt         | Required    |

Banking information in this version is profile data only. No settlement or payout workflow is included.

### 5.3 Authentication

The platform must not use ASP.NET Core Identity.

Authentication must be implemented using the application's own dedicated user, credential, role, and permission model.

#### 5.3.1 Phone Authentication

The system must support phone-number-based authentication.

Basic flow:

```text
Enter phone number
        |
        v
Request one-time code (OTP)
        |
        v
Fake SMS service
        |
        v
Enter one-time code
        |
        v
Validate OTP
        |
        v
Authenticate user
```

The OTP must:

- Have a configurable expiry time.
- Have a configurable maximum number of validation attempts.
- Be invalidated after successful validation.
- Not be stored in plaintext, if stored at all.
- Be rate-limited.

#### 5.3.2 Fake SMS Service

A simple interface must be defined:

```csharp
public interface ISmsService
{
    Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken);
}
```

The development implementation must not send real SMS messages.

This implementation can:

- Log the OTP code.
- Temporarily store it for testing purposes.
- Return a successful delivery result.

The fake service must be replaceable without changes to Application business logic.

### 5.4 Google Authentication

Google OAuth/OpenID Connect authentication must be supported.

A Google-authenticated user must be able to create or access a local User account.

The local account must be linked to the Google external identity through an appropriate External Login record.

Google authentication must not replace the local User entity.

### 5.5 Mandatory Phone Number Before Purchase

A user can authenticate via Google without providing a phone number.

However, before a payable order is created, the system must verify that the user's phone number exists and is valid.

Flow:

```text
Login with Google
     |
     v
Add to cart
     |
     v
Checkout
     |
     v
Phone number missing?
     |
    Yes
     |
     v
Collect phone number
     |
     v
Continue checkout
```

The system must not allow checkout without the mandatory phone number.

### 5.6 Cookie-Based Authentication

Authentication must be **cookie-based**.

The application must not use JWT Bearer tokens as the primary browser authentication mechanism.

#### 5.6.1 Authentication Mechanism

After successful authentication, the server must issue an authentication cookie.

The browser must send the authentication cookie automatically with subsequent requests.

Conceptually:

```text
Login / OTP verification
        |
        v
Server validates identity
        |
        v
Authentication cookie
        |
        v
Authenticated browser requests
```

#### 5.6.2 Cookie Security

Authentication cookies must use secure production settings, including where appropriate:

```text
HttpOnly = true
Secure = true
SameSite = appropriate restrictive value
```

The exact SameSite behavior must be chosen to match the application's authentication and external login requirements.

#### 5.6.3 Authentication State

The authenticated user's identity must be represented through claims inside the authentication cookie.

The cookie must not unnecessarily contain sensitive information.

Passwords, OTPs, payment secrets, and other sensitive values must never be stored inside authentication cookies.

#### 5.6.4 Logout

Logout must invalidate the authenticated session/cookie according to the configured cookie authentication mechanism.

#### 5.6.5 Google Authentication

Google authentication must still use a proper external OAuth/OpenID Connect flow.

After successful external authentication, the application must establish its own local cookie-based authenticated session.

Google access/identity tokens must not be used as the application's normal browser authentication mechanism.

#### 5.6.6 Final Authentication Flow

The final authentication flow must be:

```text
                    +----------------+
                    | Phone / Google |
                    +-------+--------+
                            |
                            v
                    Local user account
                            |
                            v
                  Cookie authentication
                            |
                            v
                Authenticated request
                            |
                            v
          Permission-based authorization
```

## 6. Roles, Permissions & Access Rules

### 6.1 Roles

Roles must be fully dynamic.

The admin panel must provide CRUD operations for roles.

#### 6.1.1 Role Fields

| Field     | Requirement       |
| --------- | ----------------- |
| Id        | Unique identifier |
| Name      | Required, unique  |
| Title     | Required          |
| IsSystem  | Required          |
| CreatedAt | Required          |
| UpdatedAt | Required          |

#### 6.1.2 Dynamic Role CRUD

Administrators with the appropriate permission must be able to:

- Create roles.
- View roles.
- Edit roles.
- Delete roles.
- Assign permissions to roles.
- Assign roles to users.

The system must not hard-code business roles such as `Admin`, `Teacher`, or `User` as the only available roles.

System roles may be seeded by the application but must still be represented by the same Role entity.

#### 6.1.3 System Roles

`IsSystem = true` means the role is required by the application or protected by application rules.

The application must prevent accidental deletion or malicious modification of system roles.

### 6.2 Permissions

Permissions must be represented dynamically in the authorization model.

Fields:

| Field     | Requirement       |
| --------- | ----------------- |
| Id        | Unique identifier |
| Name      | Required          |
| Key       | Required, unique  |
| IsSystem  | Required          |
| CreatedAt | Required          |
| UpdatedAt | Required          |

Example permissions:

```text
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
coupon.view
coupon.create
coupon.edit
coupon.delete
blog.view
blog.create
blog.edit
blog.delete
setting.view
setting.edit
```

The exact permission list may evolve along with the admin panel modules.

### 6.3 Permission–Role Assignment

Relation:

```text
Role
  |
RolePermission
  |
Permission
```

A role can have many permissions.

A permission can be assigned to many roles.

Duplicate assignments must not be allowed.

Authorization must evaluate the authenticated user's roles and the permissions assigned to those roles.

### 6.4 Role–User Assignment

Relation:

```text
User
  |
UserRole
  |
Role
```

A user can have multiple roles.

Effective permissions are the union of permissions granted through the user's roles.

If at least one assigned role grants a permission, the user is authorized for that permission, unless another explicit application-level restriction exists.

### 6.5 Permission-Based Authorization

The system must authorize administrative operations using permission keys, not by checking role names.

Wrong:

```text
if user.Role == "Admin"
```

Correct:

```text
if user.HasPermission("course.create")
```

This is required because roles are dynamic.

Authorization requirements must be enforced server-side and must not rely solely on the visibility or invisibility of UI elements.

### 6.6 Required Initial System Data

The application must seed the data required for initial operation, at minimum.

This can include:

- Initial system roles.
- Initial system permissions.
- Initial permission-role assignments.
- Required system settings.

Seeded roles remain dynamic database records and must not be replaced by hard-coded, role-name-based authorization.

### 6.7 Public Access Rules

The public website must expose content only according to its status.

Examples:

```text
Course:
    published -> public
    draft     -> not public
    hidden    -> not public

Blog:
    published/display-active -> public
    otherwise                -> not public
```

Paid course content, except explicitly free episodes, requires course ownership.

### 6.8 Course Access Rules

A user can access a course if:

```text
Course.Price == null   (free course)
```

or:

```text
the user owns the course
```

or, for a paid course:

```text
Episode.IsFree = true
```

Free preview access must not grant ownership of the whole course.

## 7. Educational Catalog (Categories, Courses, Episodes)

### 7.1 Category Management

#### 7.1.1 Fields

A category must include:

| Field        | Requirement       |
| ------------ | ----------------- |
| Id           | Unique identifier |
| Name         | Required          |
| LatinName    | Required          |
| DisplayOrder | Required integer  |
| ParentId     | Nullable          |
| IsActive     | Required boolean  |
| CreatedAt    | Required          |
| UpdatedAt    | Required          |

#### 7.1.2 Hierarchy

Categories must support a maximum of three levels.

Example:

```text
Web Development
    └── Backend
        └── Django
```

The following is invalid:

```text
Web Development
    └── Backend
        └── Django
            └── Advanced Django
```

The application must reject any creation or edit that would produce a fourth level.

#### 7.1.3 Rules

- A category must have a unique identity.
- A category can have zero or one parent.
- A root category has `ParentId = null`.
- A category cannot be its own parent.
- Circular parenting relationships are forbidden.
- Inactive categories must not be selectable as the category of new courses.
- Existing relations must not be silently removed when a category is deactivated.
- Deleting a category referenced by a course must be prevented or explicitly handled; the implementation must not leave course-category relations orphaned silently.

### 7.2 Course Management

#### 7.2.1 Course Fields

| Field             | Requirement                                                                             |
| ----------------- | --------------------------------------------------------------------------------------- |
| Id                | Unique identifier                                                                       |
| Name              | Required                                                                                |
| LatinName         | Required                                                                                |
| ShortDescription  | Required                                                                                |
| Description       | Optional/required per validation rules; supports HTML editor content                    |
| Slug              | Required, unique                                                                        |
| MainImage         | Optional                                                                                |
| Price             | Optional; null means the course is free, a non-null value (> 0) means it is paid (s7.3) |
| DiscountType      | None / Percentage / FixedAmount                                                         |
| DiscountValue     | Required when a discount is active                                                      |
| IsSaleActive      | Required                                                                                |
| DisplayStatus     | Draft / Published / Hidden                                                              |
| IsFeatured        | Required                                                                                |
| IsAmazing         | Required                                                                                |
| IntroductionVideo | Optional                                                                                |
| CreatedAt         | Required                                                                                |
| UpdatedAt         | Required                                                                                |

#### 7.2.2 Description

The full description must support HTML editor content.

The system must sanitize the HTML based on an explicit allowlist before public display.

Raw, uncontrolled user HTML must not be rendered directly.

#### 7.2.3 Slug

The course slug must:

- Be unique.
- Be URL-safe.
- Remain stable unless explicitly changed.
- Be used for public course URLs.

#### 7.2.4 Pricing

Course prices must use a decimal-compatible money type.

`float` and `double` must not be used for monetary values.

Free vs. paid is determined from `Price` (s7.3):

```text
Price == null  => free course
Price > 0      => paid course
```

The system must not require payment for access to a free course.

#### 7.2.5 Discount

Supported discount types:

```text
None
Percentage
FixedAmount
```

Percentage discounts must be within the range:

```text
0 <= DiscountValue <= 100
```

Fixed discounts must not make the final price negative.

The final payable price must be computed centrally in the Application/Domain layer.

#### 7.2.6 Sale Status

`IsSaleActive` controls whether the course can currently be purchased.

A course can be published yet not currently purchasable.

The system must not infer sale status from display status.

#### 7.2.7 Display Status

Supported values:

```text
Draft
Published
Hidden
```

Only published courses must be publicly discoverable through the regular course listing.

Draft and hidden courses must not be publicly exposed.

#### 7.2.8 Featured & Amazing

Courses can be flagged as:

- Featured.
- Amazing.

These flags correspond to specific display sections already requested.

The system must not require setting these flags in order to publish a course.

### 7.3 Product Pricing Model

Where the domain contains a `Product` concept, the pricing model must change.

The system must not store a separate `IsFree` column in the database.

Instead:

```text
Price = null
```

represents a free product.

A non-null price represents a paid product.

#### 7.3.1 Product Fields

The model must use:

```csharp
public decimal? Price { get; private set; }
```

instead of requiring:

```csharp
public bool IsFree { get; set; }
```

#### 7.3.2 IsFree Property

The domain model may expose:

```csharp
public bool IsFree => Price is null;
```

`IsFree` is a domain-derived property and must not be stored in the database.

Conceptually:

```text
Price = null
    => IsFree = true

Price > 0
    => IsFree = false
```

#### 7.3.3 Pricing Rules

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

Paid products with a zero price must not be used to represent free products unless a separate documented business requirement explicitly introduces such a state.

#### 7.3.4 Application to Course

Since `Course` currently represents the purchasable educational product, the same pricing rule applies to the Course unless a separate `Product` aggregate is introduced.

```text
Course.Price : decimal?
IsFree = derived, non-stored property
```

The access rule is:

```text
Course.Price == null
    => the course is free

Course.Price != null
    => the course is paid
```

The application must use this rule consistently in:

- Course listings.
- Course details.
- The shopping cart.
- Checkout.
- Order creation.
- Course access.
- Invoice generation.
- The admin panel.

The same rule must be applied to episode-level free-preview behavior where applicable.

### 7.4 Course Categories

A course can belong to multiple categories.

Relation:

```text
Course
   |
CourseCategory
   |
Category
```

Duplicate course/category relations must not be allowed.

Inactive categories must not be assignable to newly created or newly edited courses.

### 7.5 Course Instructors

A course can have one or more instructors.

Relation:

```text
Course
   |
CourseInstructor
   |
InstructorProfile/User
```

The system must maintain the instructor relation independently of authentication credentials.

Only users designated as instructors through their instructor profile/role assignment must be selectable as instructors in the admin panel.

### 7.6 Course Sections

Each course must contain zero or more sections.

Fields:

| Field        | Requirement       |
| ------------ | ----------------- |
| Id           | Unique identifier |
| CourseId     | Required          |
| Title        | Required          |
| DisplayOrder | Required          |
| CreatedAt    | Required          |
| UpdatedAt    | Required          |

Sections must be displayed in ascending `DisplayOrder`.

The system can compute and display:

- Episode count.
- Total video duration.

These values do not need to be persisted unless performance requirements later justify denormalization.

### 7.7 Episodes

Each section can contain zero or more episodes.

#### 7.7.1 Fields

| Field        | Requirement                        |
| ------------ | ---------------------------------- |
| Id           | Unique identifier                  |
| SectionId    | Required                           |
| Title        | Required                           |
| Description  | Optional                           |
| Video        | Optional/required per episode kind |
| Duration     | Required when a video exists       |
| IsFree       | Required                           |
| DisplayOrder | Required                           |
| CreatedAt    | Required                           |
| UpdatedAt    | Required                           |

#### 7.7.2 Ordering

Episodes must be displayed by `DisplayOrder`.

Duplicate DisplayOrder values may technically be allowed, but admin operations must keep the ordering deterministic.

#### 7.7.3 Free Episodes

A paid course can expose one or more episodes as free previews.

For example:

```text
Course.Price != null   (paid course)
Episode.IsFree = true
```

A free episode can be watched by an unauthenticated or non-purchasing user, if the course public rules allow it.

Non-free episodes of a paid course require the user to own the course.

Access checking must be centralized in the application's authorization/access layer.

#### 7.7.4 Episode Watch Status (Per-User)

Episode watch status is **per-user** and must not be stored directly on the `Episode` entity.

Each user must have their own independent status per episode. Changing one user's status must never affect another user's status.

The system must maintain the following statuses for each `(User, Episode)` pair:

| Status     | Description                                                           |
| ---------- | --------------------------------------------------------------------- |
| NotWatched | The user has not started watching the episode.                        |
| InProgress | The user has watched part of the episode but has not completed it.    |
| Watched    | The user has watched the episode completely.                          |
| Locked     | The user has no access; for example a paid episode without ownership. |

##### 7.7.4.1 Status Rules

- Status is evaluated independently for each authenticated user.
- For unauthenticated users, no status is stored; the episode is considered `NotWatched` or `Locked` based on access rules.
- The free preview episode of a paid course can be `NotWatched`, `InProgress`, or `Watched` for a non-owner, but must not be `Locked`.
- A non-free episode of a paid course that the user does not own must be `Locked`.
- After access to the course is granted, non-free episodes transition from `Locked` to `NotWatched`.
- `InProgress` is recorded when the user has started watching but has not completed the episode.
- `Watched` is recorded when the user passes the episode completion threshold; for example 90% of the video duration.
- The completion threshold must be configurable through configuration.

##### 7.7.4.2 Persistence

Watch status must be stored in a dedicated, per-user entity:

```text
UserEpisodeProgress
    UserId
    EpisodeId
    Status
    LastWatchedPosition
    FirstWatchedAt
    CompletedAt
    LastWatchedAt
    CreatedAt
    UpdatedAt
```

A unique composite constraint must be applied on `(UserId, EpisodeId)`.

##### 7.7.4.3 Access vs. Status

`Locked` is a **derived access** status, not a user-stored preference. The application must compute it based on:

- Course ownership (`UserCourse`).
- `Episode.IsFree`.
- `Course.Price` (`null` means a free course).
- The course display status.

After access is granted, the `Locked` status must automatically resolve to `NotWatched`.

##### 7.7.4.4 Progress Updates

Progress updates, such as playback position, must be validated server-side:

- `LastWatchedPosition` must not exceed `Episode.Duration`.
- Status transitions must follow these legal transitions:

```text
Locked     -> NotWatched   (after access is granted)
NotWatched -> InProgress   (on playback start)
InProgress -> Watched      (on reaching the completion threshold)
InProgress -> InProgress   (progress update)
Watched    -> Watched      (rewatching, no status regression)
```

- The `Watched` status must not regress to `InProgress` or `NotWatched` during normal playback.

### 7.8 Episode Attachments

An episode can have zero or more attachments.

Each attachment must include at least:

```text
Id
EpisodeId
FileName
StoragePath / FileReference
ContentType
Size
CreatedAt
```

The system must enforce:

- Maximum attachments per episode.
- Maximum size per attachment.
- Allowed extensions/content types.

These limits must be configuration-driven.

Example configuration:

```text
MaxAttachmentsPerEpisode
MaxAttachmentSizeBytes
AllowedAttachmentExtensions
```

These limits must be validated server-side.

### 7.9 Episode Personal Notes

User personal notes on episodes are explicitly deferred from the first implementation.

The database and application architecture must not make a future implementation impossible, but no note-management feature is required in version 1.

### 7.10 Course FAQ

A course can contain multiple FAQ items.

Fields:

| Field        | Requirement       |
| ------------ | ----------------- |
| Id           | Unique identifier |
| CourseId     | Required          |
| Title        | Required          |
| Description  | Required          |
| DisplayOrder | Required          |
| CreatedAt    | Required          |
| UpdatedAt    | Required          |

FAQ items must be displayed by `DisplayOrder`.

### 7.11 Favorites

Users must be able to mark courses as favorites.

Entity:

```text
FavoriteCourse
    UserId
    CourseId
    CreatedAt
```

A user must not be able to create a duplicate favorite relation for a single course.

Users must be able to:

- Add a course to favorites.
- Remove a course from favorites.
- View their favorite courses.

## 8. Video & File Storage (wwwroot)

### 8.1 Video Storage & Serving

#### 8.1.1 Storage Infrastructure

- The system must use the application's `wwwroot` folder to store course videos, downloadable files, and other educational files.
- Folders containing protected files under `wwwroot` must not be served publicly through the static files middleware.
- Public and anonymous access to protected files must be disabled.
- Physical paths and the folder structure details of `wwwroot` must not be exposed to the browser or to users.
- Knowing a file's name or its path in `wwwroot` must not be considered a permission to access it.
- File information, including the storage key and related specs, must be maintained in the database.
- The system must not rely solely on the unguessability of a file's address for its security.
- Access to protected resources must be controlled by the system or a trusted content-delivery layer.

#### 8.1.2 Video Format

- Course videos must be stored and served in MP4 format.
- Video playback must be possible without requiring HLS.
- The system must not require `.m3u8` manifest files or HLS video segments.
- Video playback must be performed using the browser's HTML5 video capability.
- The system must support HTTP Range requests so the browser can seek different parts of the video without downloading the whole file.
- The file-serving layer must properly handle `Range` requests and `206 Partial Content` responses.
- The response content type for MP4 files must be set correctly:
  `Content-Type: video/mp4`
- Where the browser supports it, seeking within the video and resuming playback after a pause or connection drop must be possible.

#### 8.1.3 Video Access Rules

The system must distinguish between public preview videos and protected course videos.

##### 8.1.3.1 Public Preview Videos

- Only videos explicitly designated as public previews must be watchable without authentication.
- Anonymous users must be able to watch allowed public previews without creating an account.
- Preview access must not grant access to the full paid course content or its protected files.
- Serving preview files statically and publicly must not be done, unless this decision is explicitly approved in the security requirements.
- If a preview file resides in private storage, the system must provide a controlled way to serve it to permitted users.

##### 8.1.3.2 Protected Course Videos

- Watching protected videos must require user authentication.
- The server must verify that the user has a valid, active permission to access the relevant course or lesson.
- This permission can come from a valid purchase or another explicit access rule, such as admin-approved enrollment.
- Knowing the course ID, lesson ID, video ID, or the file name in `wwwroot` must not be sufficient for access.
- Before serving a video, the user's permission must be checked server-side.
- Access to one lesson must not automatically grant access to other lessons or courses.
- Access control must be enforced server-side, not merely in the UI.

#### 8.1.4 MP4 Serving Architecture

- Protected MP4 files must be kept under `wwwroot` with direct static access blocked.
- The system must have a controlled mechanism for serving the MP4 file to the browser of permitted users.
- The implementation can choose one of the following approaches:
  1. Create an ASP.NET Core endpoint that, after checking permission, reads the MP4 file from the `wwwroot` folder and streams it to the user.

  2. Use a trusted content-delivery layer that, after checking permission, issues a short-lived signed link or access token.

- The chosen approach must comply with all access-control requirements in this document.
- If the file is served via ASP.NET Core, HTTP Range requests must be properly supported.
- The file must not be fully loaded into server memory; transfer must be streamed.
- If a signed link or content-delivery layer is used, the storage origin in `wwwroot` must remain private.
- A signed link is not inherently single-use and is usually reusable until its validity expires.
- The system must not claim that file addresses or video playback requests can be fully hidden from browser developer tools.
- The security goal is preventing unauthorized access to content, not hiding the requests the browser sends to play the video.

#### 8.1.5 Playback Permission

- The system must check the user's permission before allowing playback of a protected video to start.
- If short-lived playback credentials are used, they must have a limited expiry and be scoped to a specific allowed video, lesson, or resource.
- Where the architecture supports it, playback credentials should be tied to the authenticated user or an authorized playback session.
- The system must provide the ability to revoke or invalidate playback permission when needed.
- Playback credentials must not contain the file's physical path or other sensitive storage-infrastructure information.
- Access control must be compatible with the multiple requests needed for video playback, including Range requests.
- Using a one-time token must not disrupt normal playback, seeking, or repeated requests.
- The system must prevent IDOR; changing resource IDs in a request must not grant access to unauthorized content.

### 8.2 Course Downloads

#### 8.2.1 Download Access Control

- Protected downloadable files must be placed under `wwwroot` and served only through controlled endpoints.
- The system must check the user's authentication and access level before authorizing a download.
- The user must have a valid permission for the relevant course or lesson.
- Public preview access must not create permission to download protected files.
- The system must not offer a public, permanent address for downloading protected files.

#### 8.2.2 One-Time Download Token

- Where the business needs one-time access, the system must use one-time tokens for downloading protected files.
- The token must be generated with a secure, cryptographically random generator.
- The token must have a limited validity period.
- The token must be bound to the target file and, where relevant, to the authorized user.
- Tokens must be stored securely. Prefer storing the cryptographically hashed token in the database instead of the raw token.
- An expired, revoked, consumed, or invalidly-associated token or user must be rejected.
- Token consumption must be atomic so concurrent requests cannot consume one token multiple times successfully.
- Token usage must not bypass the primary access control on the file.
- Tokens and signed links must not be logged in plaintext in system reports.

#### 8.2.3 Large File Downloads

- The download implementation must consider large files and network interruptions.
- If resume or HTTP Range requests are supported, token consumption behavior must be compatible with multiple requests.
- Where multiple HTTP requests are needed, the one-time token should preferably be consumed to authorize a single download session.
- Subsequent requests can remain within that same authorized session until it expires or is revoked.
- The download session must stay bound to the same file and user or authorized session.
- If a signed link is used, the system must consider that the link is usually reusable until its validity expires, unless additional server-side control is implemented.
- The system must not describe an ordinary signed link as inherently single-use.

### 8.3 Upload Security & File Processing

#### 8.3.1 Uploaded File Validation

- Uploaded files must be validated server-side.
- The system must not rely solely on the client-supplied file extension or `Content-Type`.
- The maximum allowed file size must be configurable.
- Allowed formats must be checked and, where possible, the file's binary signature must also be validated.
- Uploaded images should preferably be decoded and re-encoded using a reputable image-processing library.
- Invalid images or images with unsupported formats must be rejected.
- SVG uploads must be forbidden by default unless a specific policy for sanitizing and serving them safely is implemented.
- Where a suitable service exists, uploaded files should be scanned for malware.
- A successful upload completion must not automatically make the file public.

#### 8.3.2 Video Upload

- Course videos must be stored under `wwwroot` only after passing validation and access control.
- The system must enforce configurable limits on MP4 file size and, where needed, video duration.
- The system should verify that the uploaded video is compatible with the supported playback environment.
- Uploaded files must not be executed or interpreted as server-side code.
- Video upload permission must be restricted to admins, instructors, or other authorized roles.
- The system does not require converting videos to HLS or generating HLS segments.
- Any optional video processing must be independent of the core MP4 storage and playback process.

### 8.4 HTTP, Browser & Content Serving Security

#### 8.4.1 Communication Security

- All authenticated communications in production environments must use HTTPS.
- Playback and download requests must also use HTTPS in production.
- Storage credentials must not be exposed to the browser.

#### 8.4.2 Cache Policy

- Responses for videos and protected files must have a cache policy matching the content's sensitivity.
- Caching in a reverse proxy or CDN must be configured deliberately.
- A shared cache must not expose one user's protected content to another unauthorized user.
- Responses depending on the user's permission should use `Cache-Control: private` and, if needed, `no-store`.
- If a CDN or content-delivery layer is used, cache rules and token validation must preserve per-resource access control.
- Public preview content can use a more permissive cache policy if the product requirements approve it.

#### 8.4.3 Browser Security

- The CORS policy must be configured to match the actual frontend and content-delivery architecture.
- CORS must not be considered a replacement for authentication or access control.
- Where cookie-based authentication is used, state-changing requests must be protected against CSRF.
- The server must set appropriate security headers, including `X-Content-Type-Options: nosniff`, in the right places.
- Storage-infrastructure secrets, including physical file paths, must not be exposed in frontend responses.

#### 8.4.4 Request Limiting & Event Logging

- The system should apply rate limiting to playback permission issuance, download token creation, token consumption, and other sensitive points.
- The system should identify suspicious patterns, including repeated unauthorized access attempts or excessive token consumption.
- Access-control errors and suspicious activity must be recorded in security reports.
- Passwords, access tokens, secret keys, and full download tokens must not be logged.
- Event logging must not store more sensitive personal information than necessary.

### 8.5 Access Control & Data Integrity

- Every protected video and downloadable file must be linked to its owning course or lesson.
- The server must determine the permission scope based on valid relations in the database.
- IDs submitted by the user must not, by themselves, be sufficient to prove ownership or access permission.
- Changing the course, lesson, video, or file ID must not cause unauthorized access.
- Revoking a user's permission must prevent new playback and download authorizations.
- The system must define the behavior of previously issued short-lived credentials when permission is revoked.
- File information stored in the database must remain consistent with the files present under `wwwroot`.
- File keys must be server-generated or server-validated and must not allow access to arbitrary paths in storage.

### 8.6 Security Test Requirements

The implementation must include the following tests:

1. An anonymous user can watch an allowed public preview video.
2. An anonymous user cannot access a protected course video.
3. An authenticated user without a valid entitlement cannot watch a protected video.
4. A user with a valid entitlement can play an allowed MP4 video.
5. Changing request IDs cannot allow accessing the video of another course.
6. An unauthorized user cannot access a protected file under `wwwroot` directly, without a permission check.
7. Valid HTTP Range requests are supported where needed.
8. Unauthorized users cannot obtain tokens or playback permission for protected videos.
9. Expired or invalid playback credentials are rejected.
10. Playback access control is compatible with repeated requests and seeking within the video.
11. A valid download token can be consumed per the defined policy.
12. An expired, revoked, or already-consumed download token is rejected.
13. Concurrent requests cannot consume a one-time token more than once successfully.
14. A download session, if implemented, cannot be used to fetch another file.
15. Downloading a large file completes without loading the whole file into server memory.
16. Invalid files or files with disallowed formats are rejected on upload.
17. Protected content does not become publicly accessible through misconfigured caching.
18. System logs contain no credentials, plaintext tokens, or complete signed links.
19. Revoking a user's permission prevents new playback and download authorizations.
20. Public preview access does not grant access to the paid lesson or protected downloadable file.

### 8.7 Feature Completion Criteria

This feature is considered complete when:

- The `wwwroot` storage folder for protected files is configured and direct static access to it is blocked.
- Public preview videos, where allowed, can be played without authentication.
- Playback of protected MP4 videos requires valid authentication and entitlement.
- The chosen MP4 serving approach provides access control and the required HTTP Range support.
- Video playback does not depend on HLS, `.m3u8` files, or mandatory FFmpeg conversion.
- Downloading protected files requires a valid entitlement.
- One-time download tokens, if implemented, have expiry and atomic consumption.
- Large file downloads complete without fully loading the file into server memory.
- Upload validation of files and access control to storage are implemented.
- Cache policies and communication security are configured for protected content.
- Access-control, download-token, file-upload, and content-serving tests pass.
- No sensitive storage-infrastructure information (such as physical paths) reaches the browser.

## 9. Cart, Orders & Payments

### 9.1 Purchased Courses / Enrollment

The system must maintain access to purchased courses.

Entity:

```text
UserCourse
    UserId
    CourseId
    OrderId
    PurchasedAt
```

A successful payment for a course must grant access to that course.

This operation must be idempotent.

Repeated payment callbacks or repeated processing must not create duplicate access records.

### 9.2 Existing Ownership

If a user already owns a course, the system must not create a second access entitlement for the same user/course pair.

The cart/checkout flow must prevent unnecessary re-purchase of already-owned courses.

### 9.3 Shopping Cart

The shopping cart must be stored in Redis.

It must not be persisted as the source of truth in PostgreSQL.

#### 9.3.1 Cart Item

Each cart item must include:

```text
CourseId
Quantity
```

A course must not appear more than once in a single cart.

Since the product is a course entitlement, the business rule should usually constrain quantity to one per course.

If the API receives a quantity above the allowed value, it must reject or normalize it according to a single documented rule. The implementation must use consistent, stable behavior.

#### 9.3.2 Cart Key

The Redis key must be associated with the authenticated user.

Example:

```text
cart:{userId}
```

Persisting unauthenticated users' carts is not required in this version unless explicitly added later.

#### 9.3.3 Applied Coupon

A shopping cart can have at most one applied coupon.

In addition to items, the Redis-stored cart must carry an optional coupon code:

```text
cart:{userId}
    Items      : CourseId, Quantity
    CouponCode : optional
```

Only the coupon code must be stored in the cart. Coupon discount amounts must never be stored as trusted values and must never be accepted from the browser. The server must recompute them every time the cart or checkout summary is read.

The coupon code must be removed from the cart when:

- The user removes it.
- The cart becomes empty or is cleared after a successful purchase.

If an applied coupon loses eligibility while still in the cart (for example it expires, reaches its usage cap, or the subtotal falls below the minimum order amount after an item is removed), the server must exclude its discount from all totals and inform the user that the coupon is no longer applicable. The cart must never display a discount that is not valid.

Eligibility and calculation rules are defined in s9.4.

### 9.4 Coupon Business Rules

1. Coupon validity must be re-checked server-side at order creation time, not only when the code is first applied, to prevent stale-state abuse or race conditions.
2. `UsedCount` must be incremented atomically to prevent over-consumption by concurrent requests.
3. `Code` uniqueness must be enforced in the storage layer.
4. Discount calculation logic (percentage vs. fixed amount) must live in the domain layer and must not be duplicated in application or presentation layers.
5. Per-user usage limits are not modeled directly on the Coupon entity. If needed, they must be enforced through a separate usage-tracking record associating Coupon, User, and Order.
6. Coupon fields must follow the standard audit and soft-delete requirements defined in s4.2 and s4.1.
7. A cart and an order each have at most one applied coupon.
8. A coupon is applicable only when all the following conditions hold:
   - It exists and is not soft-deleted.
   - `IsActive` is `true`.
   - The current UTC time falls within the `StartDate` to `EndDate` window (inclusive on both ends).
   - `UsageLimit` is null or `UsedCount < UsageLimit` holds.
   - `MinOrderAmount` is null or the cart subtotal is at least `MinOrderAmount`.
9. The cart subtotal is computed after course-level discounts and before the coupon. Amounts must be computed in this order:

   ```text
   Course price
       -> course-level discount (effective unit price)
       -> subtotal
       -> coupon discount
       -> payable amount
   ```

   - `Percentage`: coupon discount = subtotal × `Value` / 100.
   - `FixedAmount`: coupon discount = `Value`.
   - Computation must use `decimal` and a single documented rounding rule.

10. A coupon whose discount would bring the payable amount to zero or below is not applicable. Zero-amount orders are not supported in this version.
11. A coupon applies to the entire cart. Restricting coupons to specific courses or categories is not modeled. Free courses contribute nothing to the subtotal.
12. Coupon usage is reserved by incrementing `UsedCount` during order creation, inside the order-creation transaction and with a conditional update (`UsedCount < UsageLimit`), so concurrent requests cannot exceed the cap.
    - If the order becomes `Failed` or `Cancelled`, the reserved usage must be released exactly once, and release must be idempotent.
    - If the order becomes `Paid`, the consumed usage remains.
    - Re-processing the same payment result must never change `UsedCount`.
    - Orders left `Pending` past their expiry are cancelled per s9.6.1, which releases the reserved usage.
13. The order must store `CouponId` and `CouponDiscountAmount`. Later changes, expiry, or soft-deletion of the coupon must not alter existing orders, and historical orders must still be able to resolve their coupon.
14. Coupon codes must be trimmed and matched case-insensitively, and the storage-layer uniqueness rule (rule 3) must be consistent with that. Apply attempts must be rate-limited, and user-displayed failure messages must follow the cart-page rules in s13.3.

### 9.5 Orders

#### 9.5.1 Order

Fields:

| Field                | Requirement                                        |
| -------------------- | -------------------------------------------------- |
| Id                   | Unique identifier                                  |
| UserId               | Required                                           |
| TotalAmount          | Required; payable amount after the coupon discount |
| Status               | Required                                           |
| CreatedAt            | Required                                           |
| PaidAt               | Nullable                                           |
| ExpiresAt            | Nullable; populated while the order is Pending     |
| CouponId             | Nullable                                           |
| CouponDiscountAmount | Required; 0 when no coupon is applied              |

#### 9.5.2 Order Item

Fields:

| Field      | Requirement       |
| ---------- | ----------------- |
| Id         | Unique identifier |
| OrderId    | Required          |
| CourseId   | Required          |
| Quantity   | Required          |
| UnitPrice  | Required          |
| TotalPrice | Required          |

#### 9.5.3 Price Snapshot

The order must store the effective unit price at order creation time.

Subsequent changes in:

- Course prices.
- Course discounts.
- Coupon configuration (code, type, value, validity window, usage cap).

must not alter existing order amounts.

The order must store the coupon discount amount that was applied when the order was created.

The server must compute all monetary totals.

Client-submitted totals must never be trusted.

### 9.6 Order Status

The initial status enum must include:

```text
Pending
Paid
Cancelled
Failed
```

Status transitions must be controlled by the application's business rules.

An order must not transition to `Paid` merely because a client reports success.

#### 9.6.1 Pending Order Expiration

A `Pending` order must not remain pending forever.

When an order is created as `Pending`, `ExpiresAt` must be set to the creation time plus the configured expiration duration (`PendingOrderExpirationMinutes`).

A background job must periodically process `Pending` orders whose `ExpiresAt` has passed. For each order, it must:

- Transition the order to `Cancelled`.
- Transition its `Pending` payment record to `Cancelled`.
- Release the reserved coupon usage exactly once (s9.4).

The transition out of `Pending` must be an atomic conditional update so expiration cannot race with a payment result that makes the same order `Paid`. Whichever transition lands first wins.

The job must be idempotent and safe when multiple application instances run concurrently.

If a successful payment result arrives for an order that has already been cancelled by expiration, the system must not silently ignore it, and must not automatically grant course access or issue an invoice. It must record the payment result idempotently, log it as an error with correlation information, and make it identifiable in the orders and payments modules for admin review.

Order expiration must be recorded with structured logging.

### 9.7 Purchase Integrity

Before creating an order, the application must re-evaluate:

- Course existence.
- Course publication status, where applicable.
- Sale availability.
- Current price.
- Current discount.
- User eligibility.
- The phone number requirement.
- Existing ownership.
- Coupon validity, where a coupon is applied.

The server must never trust price, discount, or coupon amounts received from the browser.

### 9.8 Payment Methods

Payment methods must be represented by an enum.

Example:

```csharp
public enum PaymentMethod
{
    BankGateway,
    ZarinPal,
    SnappPay
}
```

No real gateway implementation is required in version 1.

The Application layer must depend on an abstraction so real providers can be implemented later without changing order business rules.

### 9.9 Payment

Payment fields:

| Field          | Requirement       |
| -------------- | ----------------- |
| Id             | Unique identifier |
| OrderId        | Required          |
| PaymentMethod  | Required          |
| Amount         | Required          |
| Status         | Required          |
| TrackingNumber | Nullable          |
| CreatedAt      | Required          |
| PaidAt         | Nullable          |

Payment statuses:

```text
Pending
Successful
Failed
Cancelled
```

The payment amount must correspond to the order's payable amount.

### 9.10 Payment Idempotency

Payment result processing must be idempotent.

If the same payment result is received multiple times, the system must not:

- Create duplicate payments.
- Create duplicate enrollments.
- Create duplicate invoices.
- Charge the user multiple times in application state.

A stable payment/provider identifier must be used where applicable.

## 10. Invoice

### 10.1 Invoice

A successful paid order must have an invoice accessible from the user's profile.

Invoice information must include:

- Invoice number.
- Invoice date.
- Customer information.
- Order reference.
- Course/order items.
- Quantity.
- Unit price.
- Discount.
- Coupon code and coupon discount, where a coupon is used.
- Final amount.
- Payment status.

The invoice must preserve a financial snapshot of the completed purchase.

The invoice's discount must include the coupon discount, and the coupon code and coupon discount amount must be preserved as snapshot values. Later coupon changes must not alter the issued invoice.

### 10.2 Invoice Data Model

The invoice must preserve a financial snapshot as of its issuance time.

The invoice must not compute historical prices by querying the current `Course` entity.

An invoice must have its own immutable financial information.

At a minimum:

```text
Invoice number
Invoice date
Customer
Customer phone
Customer email
Order reference
Payment reference
Items
Quantity
Unit price
Discount
Coupon code (when used)
Coupon discount (when used)
Tax where applicable
Subtotal
Final amount
Payment status
```

Each invoice item must have its own price snapshot.

For example:

```text
InvoiceItem
    Product/course name
    Quantity
    UnitPrice
    Discount
    TotalPrice
```

Course price changes after invoice issuance must not alter the invoice.

### 10.3 Invoice Immutability

After an invoice is finalized/issued, its financial content must not be changed through ordinary CRUD operations.

Corrections, if such a process is introduced in the future, must happen through an explicit, documented process.

The system must not silently overwrite historical invoice information due to:

- Course price changes.
- User information changes.
- Discount changes.
- Product name changes.
- Payment information changes.

### 10.4 Invoice Generation & Reporting

Invoice generation must be evaluated separately from the financial domain model.

The Invoice domain entity must represent the financial snapshot.

A reporting/PDF library must be responsible for rendering that snapshot into a document.

The system must not place PDF/reporting concerns directly inside the domain layer.

Architecture:

```text
Order / Payment
      |
      v
Invoice domain model
      |
      v
Invoice Application service
      |
      v
Invoice report generator
      |
      v
Printable/PDF invoice
```

#### 10.4.1 Candidate Libraries

The following .NET reporting/PDF technologies were evaluated:

##### 10.4.1.1 Stimulsoft Reports

Stimulsoft Reports.WEB supports ASP.NET, ASP.NET Core, MVC, Razor Pages, browser-based report design/viewing, and its documentation explicitly covers invoice reports and exporting reports to PDF.

Stimulsoft also provides an official .NET package and samples for loading, rendering, and exporting reports to PDF.

The library can also expose rendered reports through a Web API endpoint, relevant if invoice generation must later be offered as a standalone service.

Stimulsoft additionally documents support for e-invoice-related formats such as ZUGFeRD and Factur-X.

##### 10.4.1.2 QuestPDF

QuestPDF is a C# PDF-generation library with a component-based layout system.

Its official documentation includes a dedicated invoice tutorial and recommends separating invoice/document models from rendering logic.

The library can produce PDFs directly to a file, a byte array, or a Stream.

##### 10.4.1.3 FastReport .NET

FastReport .NET provides reporting and PDF export capabilities. Its official documentation includes a PDF export API and comprehensive .NET reporting documentation.

#### 10.4.2 Selection Criteria

The invoice/reporting solution must be evaluated against:

| Criterion                 | Requirement                              |
| ------------------------- | ---------------------------------------- |
| .NET compatibility        | Must support the chosen .NET version     |
| ASP.NET Core integration  | Required                                 |
| PDF export                | Required                                 |
| Invoice templates         | Required                                 |
| Persian/RTL support       | Required                                 |
| Custom fonts              | Required                                 |
| Table/layout support      | Required                                 |
| Template maintainability  | Required                                 |
| Browser preview           | Preferred                                |
| Printing                  | Preferred                                |
| Excel/Word export         | Optional                                 |
| API/server-side rendering | Preferred                                |
| Barcode/QR support        | Preferred                                |
| E-invoice support         | Preferred where relevant                 |
| Testability               | Required                                 |
| Deployment complexity     | Must be low                              |
| Licensing                 | Must be reviewed before final acceptance |

#### 10.4.3 Suggested Evaluation

The current preferred candidate is:

**Stimulsoft Reports.WEB**

The reason is that the project already has an ASP.NET Core web architecture, and this product specifically targets reporting, report design, browser viewing, PDF export, and invoice-style documents for ASP.NET/ASP.NET Core.

However, the final library choice must be made after reviewing:

- Licensing cost.
- Persian/RTL invoice rendering.
- Required fonts.
- Deployment requirements.
- Report template workflow.
- PDF output quality.
- Required output formats.
- Long-term maintenance.
- Commercial usage requirements.

QuestPDF should remain a strong alternative for when the team prefers a code-based model for producing PDFs instead of a visual report designer. Its official invoice tutorial relates the library specifically to this use case.

#### 10.4.4 Invoice Rendering Boundary

The application must define an abstraction such as:

```csharp
public interface IInvoiceRenderer
{
    Task<byte[]> RenderPdfAsync(
        InvoiceDocument document,
        CancellationToken cancellationToken);
}
```

The Domain layer must not reference Stimulsoft, QuestPDF, FastReport, or any other reporting library.

The Infrastructure layer must contain the concrete implementation.

For example:

```text
IInvoiceRenderer
       |
       +---- StimulsoftInvoiceRenderer
```

This implementation can later be replaced without changing the Invoice domain model.

## 11. SMS Service

### 11.1 Fake SMS Gateway Service

The fake SMS capability must be implemented as a **separate API project** alongside the main Whyland application.

This service must simulate the behavior of a real SMS provider API.

The intent is that the main application can communicate with an SMS API through a realistic HTTP boundary without needing a real SMS provider.

#### 11.1.1 Project Structure

The solution must have a separate project, for example:

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

`Project.Sms` must be a standalone ASP.NET Core Web API application.

This project must not contain Whyland business logic.

#### 11.1.2 SMS API

The API must expose endpoints similar to the interface typically provided by an SMS panel/provider.

At minimum:

```text
POST /api/v1/sms/send
```

The request must contain information equivalent to:

```json
{
  "to": "09120000000",
  "message": "Your verification code is 123456"
}
```

The response must contain a result similar to a real provider:

```json
{
  "success": true,
  "messageId": "..."
}
```

The exact request/response contract must be documented and versioned.

#### 11.1.3 Provider-Oriented Design

The Whyland application must communicate through an abstraction such as:

```csharp
public interface ISmsService
{
    Task<SendSmsResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken);
}
```

The Infrastructure implementation must call the separate SMS API over HTTP.

Therefore:

```text
Whyland application
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
In-Memory storage
```

This keeps the main application independent from the fake provider implementation.

#### 11.1.4 In-Memory Database

The SMS API must use an in-memory data store for the first implementation.

Exception: SMS templates (`SmsTemplate`) must be stored in the persistent database per s11.3.5; in-memory storage is used for SMS request/message records (particularly in development/test environments).

This service must store at least:

```text
SMS requests
SMS messages
Recipient
Message
Status
Message ID
CreatedAt
SentAt
```

PostgreSQL persistence is not required for the SMS simulator.

This service must be restartable without requiring database migrations.

#### 11.1.5 SMS Dashboard

The SMS service must provide a simple, HTML-based management page/dashboard.

The dashboard must let developers easily inspect:

- Incoming SMS API requests.
- Recipient phone numbers.
- Message content.
- Generated message IDs.
- SMS status.
- Request timestamps.
- Send timestamps.

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

This dashboard is intended for development/testing and does not need to be a production-grade management system.

#### 11.1.6 SMS Dashboard Requirements

The dashboard must support:

- Listing SMS requests.
- Viewing a single SMS request.
- Basic filtering by recipient/status.
- Displaying the request time.
- Displaying the message status.
- Displaying the message ID.

The dashboard can use Razor Pages or MVC.

#### 11.1.7 Real Provider Replacement

The fake SMS service must be replaceable with a real provider implementation without changes to Application-layer business logic.

Potential future implementations:

```text
FakeSmsService
KavenegarSmsService
OtherProviderSmsService
```

The provider's dedicated HTTP contract must remain outside the Domain layer.

### 11.2 SMS API Documentation & Compatibility

The SMS simulator API must be designed after reviewing the public API model of a real, existing SMS provider.

The implementation must follow common provider concepts, such as:

```text
API key / authentication
Recipient
Sender
Message
Message ID
Provider status
Request status
```

The simulator must not unnecessarily copy a specific provider's private implementation.

Its goal is to reproduce the integration pattern expected from a real SMS provider.

The selected provider's documentation must be maintained as an external reference for the implementation.

### 11.3 SMS Provider Layer & SMS Template Management

#### 11.3.1 Purpose & Overall Architecture

The SMS Provider layer must be designed and implemented as an independent, extensible module that enables sending SMS, managing SMS templates, and communicating with different SMS providers.

This layer must provide a standalone API for managing and sending SMS, and its capabilities must be usable from the main Whyland application through a standalone package.

The architecture must be designed so that the main application's Application layer does not depend on a specific SMS provider implementation, and the SMS provider can be changed without changing business logic.

Key requirements:

- Provide a standalone API for the SMS service.
- Support API versioning.
- Manage SMS templates.
- Define replaceable template parameters.
- Assign a unique code to each SMS template.
- Store templates and related information in the database.
- Provide a View for managing and viewing templates in the SMS layer.
- Create a standalone package for using SMS Provider capabilities in the main application.
- Use abstractions to keep the consumer independent from the provider implementation.
- Use Refit for HTTP communication in the API client implementation.
- Enable replacing the fake provider with real providers.

#### 11.3.2 Project Structure & Responsibility Separation

The solution must include a standalone project for the SMS service and a reusable package for connecting consumer applications to this service.

Suggested structure:

```text
src/
│
├── Project.Domain/
├── Project.Application/
├── Project.Infrastructure/
├── Project.Web/
│
├── Project.Sms/
│   ├── Project.Sms.Api/
│   ├── Project.Sms.Application/
│   ├── Project.Sms.Domain/
│   ├── Project.Sms.Infrastructure/
│   └── Project.Sms.Web/
│
└── Project.Sms.Provider/
    ├── Abstractions/
    ├── Contracts/
    ├── Client/
    └── DependencyInjection/
```

The above structure is a suggestion and can change according to the solution's final architecture.

##### 11.3.2.1 Project.Sms

This part is responsible for providing the standalone SMS service and must offer:

- A versioned API.
- Processing of send-SMS requests.
- SMS template management.
- Storage of template information and SMS requests.
- Generation of unique identifiers.
- A management View for viewing and managing templates and messages.
- Request/message status management.

This project must not contain Whyland-specific business logic.

##### 11.3.2.2 Project.Sms.Provider

This project must be designed as a standalone, reusable package so the main application can use the SMS service without implementing HTTP communication directly.

The responsibilities of this package are:

- Providing abstractions for SMS service capabilities.
- Defining implementation-independent contracts.
- Providing the SMS API client using Refit.
- Managing service connection settings.
- Providing DI registration extensions.
- Mapping API responses to the package's public models.

This package must not depend on the internal implementation of `Project.Sms` or on any specific SMS provider's details.

#### 11.3.3 Standalone API Offered by the SMS Provider

The SMS service must offer a standalone API for consumption by the main application and other clients.

This API must support sending SMS based on a template and input parameters.

Suggested primary endpoints:

| Method   | Endpoint                       | Description                                |
| -------- | ------------------------------ | ------------------------------------------ |
| `POST`   | `/api/v1/sms/send`             | Send SMS using raw text or a template      |
| `GET`    | `/api/v1/sms/{messageId}`      | Get information about one SMS              |
| `GET`    | `/api/v1/sms/templates`        | Get the SMS template list                  |
| `GET`    | `/api/v1/sms/templates/{code}` | Get one template's info by its unique code |
| `POST`   | `/api/v1/sms/templates`        | Create an SMS template                     |
| `PUT`    | `/api/v1/sms/templates/{code}` | Edit an SMS template                       |
| `DELETE` | `/api/v1/sms/templates/{code}` | Delete an SMS template, where permitted    |

Template-management endpoints must be protected against unauthorized access per the security requirements.

Each endpoint's request/response contract must be documented and included in API versioning.

#### 11.3.4 SMS Template Management

The SMS service must support reusable templates for generating SMS content.

Each template must include main text and a set of replaceable parameters.

For example, consider this template:

**Template name:** Phone number verification

**Unique code:** `SMS-OTP-001`

**Template text:**

```text
Your verification code: {{code}}
This code is valid for {{expirationMinutes}} minutes.
```

**Template parameters:**

```json
{
  "code": "123456",
  "expirationMinutes": 5
}
```

**Final SMS text:**

```text
Your verification code: 123456
This code is valid for 5 minutes.
```

##### 11.3.4.1 Required Template Information

Each SMS template must include at least:

| Field         | Description                                      |
| ------------- | ------------------------------------------------ |
| `Id`          | Template internal identifier                     |
| `Code`        | Unique code usable for referencing the template  |
| `Name`        | Displayable template name                        |
| `Content`     | The SMS template text                            |
| `Parameters`  | Definition of the template's required parameters |
| `Description` | Optional description                             |
| `IsActive`    | Template enabled/disabled state                  |
| `CreatedAt`   | Template creation time                           |
| `UpdatedAt`   | Last edit time                                   |

The template's unique code must be unique within the system's intended scope and used to identify the template in API requests.

##### 11.3.4.2 Template Parameter Definition

Each template must be able to define its required parameters.

A parameter definition can include:

- Parameter name.
- Data type.
- Required vs. optional.
- Default value, if any.
- Parameter description.

For example:

```json
{
  "code": {
    "type": "string",
    "required": true,
    "description": "Verification code"
  },
  "expirationMinutes": {
    "type": "integer",
    "required": true,
    "description": "Code validity duration in minutes"
  }
}
```

The service must validate the submitted parameters when a template is used.

If a required parameter is missing, or a required template parameter is not replaceable, the request must be rejected with a specific, documented error.

##### 11.3.4.3 Sending SMS via Template

A send-SMS request must support referencing a template by its unique code and submitting parameter values.

Example request:

`POST /api/v1/sms/send`

```json
{
  "to": "09120000000",
  "templateCode": "SMS-OTP-001",
  "parameters": {
    "code": "123456",
    "expirationMinutes": 5
  }
}
```

In this case, the service must perform these steps:

1. Find the template by `templateCode`.
2. Check that the template is active.
3. Validate the input parameters.
4. Substitute parameter values into the template text.
5. Record the send-SMS request.
6. Process the generated message according to the simulated provider behavior.
7. Return the operation result with the message ID.

Example success response:

```json
{
  "success": true,
  "messageId": "sms_01JABC123XYZ",
  "status": "Sent"
}
```

The message ID must be unique per generated message.

**Note:** recording a request and creating a message in the simulator does not necessarily mean the SMS was actually delivered to the recipient's phone. The API statuses must clearly reflect this difference.

#### 11.3.5 Storing Templates & Messages

SMS template information must be stored in the database so it survives service restarts.

For the first implementation, the project's existing database or a separate database for the SMS service can be used.

The data structure must cover at least the following entities.

##### 11.3.5.1 SmsTemplate Entity

Responsible for maintaining SMS templates.

```text
SmsTemplate
-----------
Id
Code
Name
Content
Parameters
Description
IsActive
CreatedAt
UpdatedAt
```

The `Code` field must have a uniqueness constraint in the database.

The `Parameters` field can be implemented as JSON or a relational structure, depending on the storage technology.

##### 11.3.5.2 SmsRequest Entity

Responsible for maintaining information about requests received by the API.

```text
SmsRequest
----------
Id
RequestId
Endpoint
RequestPayload
ReceivedAt
ResponseStatus
ErrorCode
ErrorMessage
```

This entity enables inspecting incoming requests and possible errors.

If full request content is recorded, sensitive information must be handled or removed according to the system's security policies.

##### 11.3.5.3 SmsMessage Entity

Responsible for maintaining information about generated SMS messages.

```text
SmsMessage
----------
Id
MessageId
TemplateCode
Recipient
Message
Status
CreatedAt
SentAt
```

If needed, additional information such as `ProviderStatus`, `ErrorCode`, and `ErrorMessage` can be added to this entity.

Status values must be defined in the API contract; for example:

- `Pending`
- `Sent`
- `Failed`

The meaning of each status must be clear. In particular, the `Sent` status in the simulator must represent the service's defined behavior and must not be interpreted as confirmation of actual SMS delivery.

#### 11.3.6 SMS Management Dashboard & Views

The SMS layer must provide an HTML interface for viewing and managing SMS templates and inspecting recorded requests and messages.

This interface can be implemented using Razor Pages or ASP.NET Core MVC.

##### 11.3.6.1 Template Management

The dashboard must allow viewing and managing SMS templates.

Required capabilities:

- View the template list.
- View each template's unique code.
- View the template text and parameters.
- View the template's enabled/disabled state.
- Create a new template.
- Edit a template.
- Enable/disable a template.
- View creation and last-edit timestamps.

##### 11.3.6.2 Viewing Messages & Requests

The dashboard must display information about recorded requests and messages.

Minimum visible information:

- Request ID.
- Message ID.
- Recipient number.
- Template code used.
- Final message text.
- Message status.
- Request receipt time.
- Message creation time.
- Simulated send time, if present.
- Error information if the operation failed.

##### 11.3.6.3 Filtering & Detail Views

The dashboard must support:

- Filtering by recipient number.
- Filtering by message status.
- Filtering by template code.
- Viewing a message's details.
- Viewing an API request's details.
- Viewing operation timestamps.

This dashboard is primarily intended for development and testing. However, access to it must be controlled and sensitive information must not be exposed unrestrictedly to unauthorized users.

#### 11.3.7 Standalone SMS Provider Package

The main Whyland application must not manage HTTP API details or provider implementation directly to use the SMS service.

Therefore, a standalone package with the suggested name `Project.Sms.Provider` must be created.

This package must enable using the SMS service capabilities through public abstractions.

##### 11.3.7.1 Abstraction Definition

The package must provide a public interface for sending SMS.

For example:

```csharp
public interface ISmsService
{
    Task<SendSmsResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken);
}
```

To support templates, an additional contract can be defined:

```csharp
public interface ISmsTemplateService
{
    Task<SendSmsResult> SendTemplateAsync(
        string phoneNumber,
        string templateCode,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken cancellationToken);
}
```

The send result model must also be implementation-independent:

```csharp
public sealed class SendSmsResult
{
    public bool Success { get; init; }

    public string? MessageId { get; init; }

    public string? Status { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }
}
```

These contracts are suggested samples and must be aligned with the final API contract.

The package's public contracts must not depend on `Project.Sms`'s internal models or a specific provider's models.

##### 11.3.7.2 Main Application Responsibility

After the package is added, the main application must interact with the SMS service through the public interfaces.

The communication flow is:

```text
Project.Web / Project.Application
                |
                v
            ISmsService
                |
                v
       SMS Provider Package
                |
                v
          Refit Client
                |
                v
         Project.Sms API
                |
                v
     SMS Application Services
                |
                v
        Database / Storage
```

In this structure, the main application is independent from the communication implementation details.

Consequently, changing the service address, connection settings, or client implementation must not change the main application's business logic.

#### 11.3.8 Using Refit for HTTP Communication

The API client implementation in the `Project.Sms.Provider` package must use the Refit library instead of using `HttpClient` directly to define and call endpoints.

Refit must define the SMS service's HTTP contract as an interface and handle sending requests.

Example:

```csharp
using Refit;

public interface ISmsApiClient
{
    [Post("/api/v1/sms/send")]
    Task<ApiResponse<SendSmsApiResponse>> SendAsync(
        [Body] SendSmsApiRequest request,
        CancellationToken cancellationToken);
}
```

The `SendSmsApiRequest` and `SendSmsApiResponse` models must be defined in the package's communication-contracts section.

If the contracts for raw-text sending and template-based sending differ, the endpoints/contracts matching the final API must be used.

##### 11.3.8.1 Client Registration in DI

The package must allow registering the service and client through an extension method.

For example:

```csharp
services
    .AddRefitClient<ISmsApiClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(
            configuration["SmsProvider:BaseUrl"]!);
    });
```

The service base address must come from configuration and must not be hard-coded.

Also, authentication settings, timeouts, and error-handling policies must be configurable through package settings where needed.

##### 11.3.8.2 Mapping API Contracts to Public Models

The package must inspect the response received from Refit and convert it to the public `SendSmsResult` model.

This conversion must consider:

- Successful API responses.
- Failed API responses.
- Validation errors.
- Communication errors.
- Timeouts.
- Invalid or unprocessable responses.

The main application's business logic must not deal directly with error types or Refit-specific models.

**Architectural note:** Refit uses `HttpClient` under the hood. The requirement of this section is to use Refit to define and invoke the HTTP contract, not to eliminate `HttpClient` from the communication infrastructure entirely.

#### 11.3.9 SMS Provider Independence

The architecture must enable replacing the SMS service implementation without changes to the main application's business logic.

Possible implementations:

```text
ISmsService
    |
    ├── FakeSmsService
    |
    ├── KavenegarSmsService
    |
    └── OtherProviderSmsService
```

These implementations must follow the same public contract; however, each provider can keep its own communication contract and models in the Infrastructure layer.

For example:

- `FakeSmsService` can be used for testing or the development environment.
- `KavenegarSmsService` can communicate with the Kavenegar provider's API.
- Future implementations can connect to other providers.

In the package-based architecture, the main application depends only on the abstraction, and the implementation is chosen via Dependency Injection and Configuration.

Providers' dedicated HTTP contracts must not leak into the Domain layer or the main application's business logic.

#### 11.3.10 SMS API Versioning

Versioning the SMS service's API is mandatory and must be reflected in endpoint design and communication contracts from the start of implementation.

Path-based versioning is suggested as the primary approach.

For example:

```text
/api/v1/sms/send
/api/v1/sms/templates
/api/v1/sms/templates/{code}
```

If incompatible contract changes occur, a new version will be released:

```text
/api/v2/sms/send
/api/v2/sms/templates
```

##### 11.3.10.1 Versioning Requirements

- Each version must have a specific request/response contract.
- Incompatible changes must not be applied to the existing contract without announcing a new version.
- Each version's documentation must be independent and reviewable.
- The version in use must be specified in the SMS Provider package.
- API version upgrades must be performed without directly coupling business logic to versioning details.
- Where multiple versions are supported simultaneously, each version's behavior and support period must be documented.

##### 11.3.10.2 API Version Coordination with the Package

The `Project.Sms.Provider` package must be compatible with a specific version of the API contract.

Changing the API version must not change package behavior silently.

Where supporting the new version is needed, a new package version or an implementation compatible with the new API version can be provided.

The SMS service's API versioning and the NuGet package versioning are separate concerns and must be managed separately.

#### 11.3.11 Security & Error Handling

The SMS service and the consuming package must observe at least the following security and error-handling requirements:

- Ability to authenticate API requests via an API key or another suitable mechanism.
- Keeping authentication information in secure configuration.
- Preventing API keys and other sensitive information from being logged.
- Validating phone numbers, template codes, and submitted parameters.
- Checking template activation before generating a message.
- Handling communication errors and timeouts.
- Offering a standard, processable error response.
- Recording operational errors for review and troubleshooting.
- Preventing sensitive data leaks in error details.

##### 11.3.11.1 Summary of Design Decisions

| Topic                   | Suggested decision                                     |
| ----------------------- | ------------------------------------------------------ |
| SMS service structure   | Standalone ASP.NET Core Web API project                |
| Consuming package       | `Project.Sms.Provider`                                 |
| Public contract         | Provider-independent interfaces and DTOs               |
| API client              | Refit                                                  |
| API versioning          | Path-based versioning, e.g. `/api/v1`                  |
| Template storage        | Persistent database                                    |
| Request/message storage | Database or environment-appropriate storage            |
| Template identification | Unique code with a DB uniqueness constraint            |
| Message identification  | Unique ID generated per message                        |
| Dashboard               | Razor Pages or ASP.NET Core MVC                        |
| Provider selection      | Via abstraction and Dependency Injection               |
| Documentation           | OpenAPI/Swagger and contract documentation             |
| Security                | Authentication, input validation, dashboard protection |

**Important note on storage:** given the need to maintain templates in the database, using persistent storage for `SmsTemplate` is essential. This is an exception to the original in-memory storage requirement (s11.1.4).

Final decision: templates are stored in the persistent database, and using in-memory storage for SMS records in test environments remains available.

## 12. Public Content (Blog, FAQ, Site Settings)

### 12.1 Blog

#### 12.1.1 Blog Category

The platform must support blog categories.

Blog category fields:

| Field        | Requirement       |
| ------------ | ----------------- |
| Id           | Unique identifier |
| Name         | Required          |
| LatinName    | Required          |
| Slug         | Required, unique  |
| DisplayOrder | Required integer  |
| ParentId     | Nullable          |
| IsActive     | Required boolean  |
| CreatedAt    | Required          |
| UpdatedAt    | Required          |

Blog category rules:

- Blog categories can be hierarchical; the three-level depth cap is not enforced for them unless explicitly configured.
- A blog category cannot be its own parent.
- Circular parenting is forbidden.
- Inactive categories must not be selectable for new posts.
- Existing relations must not be silently removed when a category is deactivated.
- Deleting a blog category referenced by a post must be prevented or explicitly handled and must not orphan the post-category relation.
- Blog categories follow the general soft-delete and audit rules defined in s4.1 and s4.2.

#### 12.1.2 Blog Post Fields

| Field            | Requirement       |
| ---------------- | ----------------- |
| Id               | Unique identifier |
| Title            | Required          |
| ShortDescription | Required          |
| Content          | Required          |
| AuthorId         | Required          |
| CategoryId       | Optional          |
| Image            | Optional          |
| DisplayOrder     | Required          |
| DisplayStatus    | Required          |
| Slug             | Required, unique  |
| CreatedAt        | Required          |
| UpdatedAt        | Required          |

The author must reference a User.

Only content flagged for public display must appear on the public website.

### 12.2 Public FAQ

The platform must support public FAQs.

Fields:

| Field        | Requirement       |
| ------------ | ----------------- |
| Id           | Unique identifier |
| Title        | Required          |
| Description  | Required          |
| DisplayOrder | Required          |
| IsActive     | Required          |
| CreatedAt    | Required          |
| UpdatedAt    | Required          |

Public FAQs are separate from course FAQs.

### 12.3 Site Settings

The system must provide dynamic site settings.

Examples:

#### 12.3.1 General

- Site title.
- Logo.
- Favicon.
- Site description.

#### 12.3.2 SEO

- Default meta title.
- Default meta description.
- Default keywords when needed.
- Open Graph image.
- Other required SEO metadata.

#### 12.3.3 Contact

- Phone.
- Email.
- Address.

#### 12.3.4 Dynamic Content

Content that must be dynamically managed by the website must be configurable through the admin panel.

The implementation must avoid hard-coding editable site content inside Controllers or Views.

## 13. UI: Public Website & Admin Panel

### 13.1 Public Website

The public website must be implemented with ASP.NET Core MVC.

#### 13.1.1 Home Page

The home page must support the requested content/course sections, including:

- Featured courses.
- Amazing courses.
- Other course sections required by the configured site content.
- Recent articles.
- Public FAQs where configured.

#### 13.1.2 Course Listing

The course-listing page must display published courses.

The page must support category-aware course discovery.

It must display at least:

- Course title.
- Main image.
- Short description.
- Price.
- Discount/final price when present.
- Free/paid status.

#### 13.1.3 Course Details

The course-details page must display:

- Course title.
- Main image.
- Short description.
- Full description.
- Categories.
- Instructor information.
- Price.
- Discount.
- Introduction video.
- Sections.
- Episodes.
- Episode durations.
- Free episodes.
- Course FAQ.
- Per-episode watch status for the current user (`NotWatched` / `InProgress` / `Watched` / `Locked`).
- Course progress summary for the current user (for example, watched episodes out of total).

#### 13.1.4 Blog Listing

The blog-listing page must display public blog posts.

#### 13.1.5 Blog Details

The article page must display:

- Title.
- Short description.
- Content.
- Author.
- Image.
- Publication/display status.

#### 13.1.6 Authentication Pages

Login and registration can be combined into a unified authentication flow.

The authentication experience must support:

- Phone OTP.
- Google login.
- Password recovery where a local password exists.

#### 13.1.7 Forgot Password

The system must provide a password-recovery flow.

The recovery mechanism must use a secure, time-limited token or an equivalent mechanism.

Password reset tokens must:

- Be time-limited.
- Be single-use.
- Be invalidated after successful reset.

### 13.2 User Profile

The profile area must include:

#### 13.2.1 User Information

Display of the current user's profile and account information.

#### 13.2.2 Edit Information

Users can update allowed profile fields.

Sensitive identity fields such as the national code may require extra validation or become non-editable after verification if a verification mechanism is introduced.

#### 13.2.3 Change Password

Authenticated users with a local password must be able to change their password.

#### 13.2.4 Favorite Courses

Display the user's favorite courses.

#### 13.2.5 Purchased Courses

Display courses the user has access to.

#### 13.2.6 Payment History

Display the user's payment history.

#### 13.2.7 Invoices

Users must be able to view/download their invoices where supported by the presentation layer.

#### 13.2.8 Episode Progress

The user profile area can display the user's watch progress across courses, including:

- Progress percentage per course.
- Each episode's status (`NotWatched` / `InProgress` / `Watched`).
- Last watched episode and when.

`Locked` episodes must be displayed as locked and must not expose progress controls to the user.

### 13.3 Cart Page

The cart page must display:

- Course.
- Quantity.
- Unit price.
- Discount.
- Row total.
- Subtotal (sum of row totals after course-level discounts).
- Coupon discount, when a coupon is applied.
- Cart grand total (payable amount after the coupon discount).

Users must be able to remove items.

Quantity changes must honor course-purchase rules.

The cart page must allow the authenticated user to apply a coupon. It must include:

- A coupon-code input with an apply action, shown while no coupon is applied.
- The applied coupon code with a remove action.
- A clear message when a code is not applicable.

Applying or removing a coupon must immediately update the displayed totals with server-computed values.

Failure messages must be limited to reasons the user can act on (for example, the minimum order amount is not met). Any other failure (unknown code, inactive, expired, not-yet-started, or exhausted) must use a generic message so valid codes cannot be discovered by guessing.

Coupon apply attempts must be rate-limited.

### 13.4 Checkout Process

The checkout process must consist of exactly three conceptual steps.

```text
1. Information
        |
        v
2. Payment
        |
        v
3. Payment result
```

#### 13.4.1 Step 1 — Information

The system must validate the required customer information.

The phone number is mandatory before payment.

The order summary must show the subtotal, applied coupon code (if any), coupon discount, and payable amount.

The user must be able to apply, change, or remove the coupon at this stage under the same rules as the cart page. The coupon remains mutable until the pending order is created in Step 2.

#### 13.4.2 Step 2 — Payment

The user must select one of the supported payment-method enum values.

The application must create the required pending payment/order state.

When the cart has an applied coupon, order creation must additionally:

- Re-validate the coupon server-side at that moment (s9.4).
- Recompute the coupon discount and payable amount.
- Store the coupon reference and coupon discount amount as a snapshot on the order.
- Reserve one coupon usage atomically, inside the order-creation transaction.

The payment amount must equal the order's payable amount after the coupon discount.

If the coupon is no longer valid at order creation, no order or payment must be created. The user must be returned to Step 1 with a message and recalculated totals, and must explicitly choose to continue. The system must not silently continue at a higher price.

After the pending order is created, its coupon and amounts are frozen and must not change.

#### 13.4.3 Step 3 — Payment Result

The system must display:

- Successful payment.
- Failed payment.

The result must show the order summary, including the coupon code and coupon discount when a coupon was used.

For a successful payment:

```text
Successful payment
        |
        v
Order = Paid
        |
        v
Grant course access
        |
        v
Create/finalize invoice
```

This entire flow must be transactionally consistent where database operations are involved.

The invoice created for a paid order must preserve the coupon snapshot per s10.1.

If the order becomes `Failed` or `Cancelled`, the coupon usage reserved for it must be released exactly once. The cart, including its applied coupon, must be preserved for the user's retry, and the coupon remains applied only if it is still eligible.

### 13.5 Admin Panel

The admin panel must be implemented with Razor Pages.

All administrative operations must be protected by permissions.

#### 13.5.1 Required Admin Modules

The panel must provide management of:

- Categories.
- Courses.
- Course sections.
- Episodes.
- Episode attachments.
- Course FAQs.
- Users.
- User profiles.
- Instructors.
- Roles.
- Permissions.
- Permission-role assignment.
- Role-user assignment.
- Orders.
- Payments.
- Coupons.
- Blog posts.
- Public FAQs.
- Site settings.

#### 13.5.2 Coupon Management

The admin panel must provide coupon management protected by `coupon.view`, `coupon.create`, `coupon.edit`, and `coupon.delete` permissions.

The coupon list must show the code, type, value, usage (`UsedCount` / `UsageLimit`), start date, end date, `IsActive`, and a derived status (active, scheduled, expired, exhausted, or inactive). It must support searching by code and filtering by status.

Coupon form fields:

| Field          | Requirement                                                                         |
| -------------- | ----------------------------------------------------------------------------------- |
| Code           | Required; trimmed; unique, compared case-insensitively                              |
| Type           | Required; Percentage or FixedAmount                                                 |
| Value          | Required; Percentage: greater than 0 and less than 100; FixedAmount: greater than 0 |
| MinOrderAmount | Optional; must not be negative                                                      |
| UsageLimit     | Optional; empty means unlimited; when set, greater than 0                           |
| UsedCount      | Read-only                                                                           |
| StartDate      | Required                                                                            |
| EndDate        | Required; after StartDate                                                           |
| IsActive       | Required                                                                            |

Coupon management rules:

- `UsedCount` must never be edited manually.
- `UsageLimit` must not be set below the current `UsedCount`.
- Editing, deactivating, or deleting a coupon must not alter existing orders or invoices (s9.4).
- Deactivating a coupon is the normal way to stop its use. Deleting a coupon is a soft delete, and a coupon used by any order must never be physically deleted.
- The order details in the orders module must display the coupon code and coupon discount amount.
- Audit fields must be set server-side and never supplied from the client.

### 13.6 CRUD Requirements

Where an entity is defined as administratively manageable, the admin panel must provide the appropriate:

- Create.
- Read/List.
- Update.
- Delete.

CRUD operations must respect:

- Validation.
- Authorization.
- Referential integrity.
- System-entity constraints.
- Audit timestamps.

Delete operations that could destroy important historical/financial data must use an appropriate business rule rather than a blind physical delete.

For example, paid orders and successful payments must not be physically deleted through ordinary admin CRUD.

### 13.7 Error Handling

The public website and admin panel must support:

#### 13.7.1 HTTP 400

For invalid client/application input, where applicable.

#### 13.7.2 HTTP 403

Displayed when an authenticated user lacks the required permission.

#### 13.7.3 HTTP 404

Displayed when a requested resource does not exist or is not publicly accessible.

#### 13.7.4 HTTP 500

Displayed for unexpected server-side errors.

Internal exception details, stack traces, database errors, and secrets must never be shown to end users.

All unexpected errors must be logged with proper correlation information.

## 14. Observability

### 14.1 Observability

The system must provide:

- Logs.
- Distributed traces.
- Metrics.

The target stack is:

```text
Application
    |
    +-- Serilog
    |
    +-- OpenTelemetry
           |
           +-- Logs    -> Loki
           +-- Traces  -> Tempo
           +-- Metrics -> Prometheus
                            |
                            v
                         Grafana
```

Grafana must provide unified dashboards for operational and business observability.

### 14.2 Logging Requirements

Logs must be structured.

At a minimum, relevant request logs must include:

- Timestamp.
- Level.
- Message.
- Exception, when present.
- Trace ID.
- Span ID, when available.
- User ID, when available.
- HTTP method.
- Request path.
- Status code.
- Duration.

Sensitive values must not be logged.

The system must not log:

- Passwords.
- OTP values in production.
- Authentication tokens.
- Payment secrets.
- Full sensitive personal information.

The fake SMS service may log OTPs only in development/test environments.

### 14.3 Distributed Tracing Requirements

OpenTelemetry must instrument relevant application operations.

Traces must cover:

- Incoming HTTP requests.
- Database operations.
- Redis operations.
- External service calls.
- Authentication operations.
- Payment operations.

Trace IDs must be correlated with application logs.

### 14.4 Metrics Requirements

The system must expose application and business metrics.

#### 14.4.1 Default/System Metrics

At a minimum:

- Request count.
- Request duration.
- Active requests, where supported.
- HTTP status distribution.
- Error rate.
- Process/runtime metrics.
- Database performance indicators.
- Redis performance indicators, where available.

#### 14.4.2 Business Metrics

The system must expose:

- Course views.
- Purchases.
- Successful payments.
- Failed payments.
- Enrollment counts.
- Conversion rate, where computable.

#### 14.4.3 Slow Operation Metrics

The system must make slow operations observable, including:

- Slow HTTP requests.
- Slow database queries.
- Slow external service calls.

Thresholds must be configuration-driven.

### 14.5 Grafana Dashboards

The project must provide dashboards for at least:

#### 14.5.1 Application

- Request rate.
- Response time.
- Error rate.
- HTTP 4xx.
- HTTP 5xx.

#### 14.5.2 Business

- Course views.
- Purchases.
- Successful payments.
- Failed payments.
- Conversion rate.

#### 14.5.3 Logs

- Error logs.
- Warning logs.
- Application logs.

#### 14.5.4 Traces

- Slow requests.
- Failed requests.
- Trace details.

Prometheus must act as the metrics backend, Loki as the log backend, and Tempo as the tracing backend.

## 15. Security & Non-Functional Requirements

### 15.1 Security Requirements

#### 15.1.1 Password Security

Passwords must be securely hashed using a modern hashing mechanism.

Passwords must never be stored in plaintext.

#### 15.1.2 Authentication Security

Authentication tokens/cookies must:

- Be protected against tampering.
- Use secure cookie settings in production.
- Have appropriate expiry times.
- Support logout/invalidation according to the chosen authentication mechanism.

#### 15.1.3 Authorization

All protected operations must be authorized server-side.

Hiding a button in the admin UI must never be considered sufficient authorization.

#### 15.1.4 Input Validation

All external input must be validated.

This includes:

- Form input.
- Query parameters.
- Route parameters.
- File uploads.
- Payment results.
- OAuth responses.

#### 15.1.5 HTML Content

HTML-editor content must be sanitized.

#### 15.1.6 File Uploads

Attachments must be validated based on:

- Extension.
- Content type, where possible.
- Size.
- Count.
- Storage rules.

Uploaded files must not automatically become executable content.

### 15.2 Non-Functional Requirements

#### 15.2.1 Performance

The application must:

- Use asynchronous I/O.
- Avoid blocking calls.
- Use pagination for potentially large admin/public lists.
- Avoid N+1 database queries.
- Monitor slow database queries.
- Monitor slow HTTP requests.

#### 15.2.2 Scalability

The application must remain stateless in the web-server layer, as much as possible.

Cart/session state must not depend on local server memory.

Redis must be used for shared temporary state.

#### 15.2.3 Maintainability

The codebase must:

- Respect Clean Architecture boundaries.
- Separate Domain, Application, Infrastructure, and Presentation concerns.
- Use dependency injection.
- Use automated tests for important business rules.
- Avoid duplicating business logic.

#### 15.2.4 Reliability

Critical operations must be idempotent where request repetition is possible.

Payment processing must specifically be idempotent.

#### 15.2.5 Security

The application must follow secure defaults for:

- Authentication.
- Authorization.
- Password storage.
- File uploads.
- HTML rendering.
- Secrets.
- Logging.
- Input validation.

## 16. Testing, Acceptance Criteria & Definition of Done

### 16.1 Testing Requirements

The project must include:

#### 16.1.1 Unit Tests

For:

- Pricing.
- Discount calculations.
- Category hierarchy validation.
- Course access rules.
- Order calculations.
- Permission evaluation.
- Payment state transitions.

#### 16.1.2 Integration Tests

For:

- PostgreSQL persistence.
- Repository behavior.
- Unit of Work transactions.
- Redis cart operations.
- Authentication flows.
- Payment result processing.

#### 16.1.3 End-to-End Tests

For critical flows:

```text
Register/Login
       ↓
Browse courses
       ↓
Add to cart
       ↓
Checkout
       ↓
Payment
       ↓
Course access
       ↓
Invoice
```

### 16.2 Acceptance Criteria — Core Flows

#### 16.2.1 Course Purchase

The following must hold:

1. A user can browse published courses.
2. A paid course can be added to the Redis cart.
3. Checkout requires a phone number.
4. The server computes the order price.
5. An order is created.
6. A payment record is created.
7. A successful payment transitions the order to `Paid`.
8. The user receives access to the course.
9. An invoice becomes available.
10. Re-processing the same payment result does not duplicate access or invoice status.

#### 16.2.2 Dynamic Roles

The following must hold:

1. An authorized admin can create a role.
2. The admin can edit the role.
3. The admin can assign permissions.
4. The admin can assign the role to users.
5. The user receives the role's permissions.
6. Authorization is based on permission keys.
7. The admin can delete a role that is not protected as a system role.

#### 16.2.3 Course Structure

The following must hold:

1. A course can contain multiple sections.
2. A section can contain multiple episodes.
3. Sections are ordered.
4. Episodes are ordered.
5. Episode durations are available.
6. A paid course can have multiple free episodes.
7. Non-free episodes require course ownership.

#### 16.2.4 Coupon Checkout

The following must hold:

1. A coupon code can be applied to a cart containing paid courses.
2. The server computes the coupon discount and payable amount; client-provided amounts are ignored.
3. An unknown, inactive, expired, not-yet-started, exhausted, or below-minimum code is rejected with one message.
4. The user can remove the applied coupon from the cart.
5. The coupon is re-validated at order creation, and an invalid coupon prevents order creation.
6. The order stores the coupon reference and coupon-discount snapshot, and the payment amount equals the payable amount.
7. Concurrent checkouts cannot push `UsedCount` past `UsageLimit`.
8. A `Failed` or `Cancelled` order releases its reserved coupon usage exactly once.
9. The invoice of a paid order shows the coupon code and coupon discount.
10. Re-processing the same payment result does not change `UsedCount`.
11. A `Pending` order past its expiry is cancelled and its reserved coupon usage is released.
12. A successful payment result arriving after expiry is recorded and flagged for review, without automatically granting access.
13. An admin can create, edit, deactivate, and soft-delete coupons and cannot set an invalid value or a usage cap below the current usage.

### 16.3 Definition of Done

A feature is considered complete when:

1. Domain requirements are implemented.
2. Application Commands/Queries are implemented.
3. Validation is implemented.
4. Read operations go through the read-repository boundary.
5. Write operations go through the write-repository boundary (and Unit of Work where needed).
6. Read repositories use Dapper.
7. Write repositories use EF Core.
8. No generic repository is introduced.
9. Required audit fields are populated.
10. Soft delete is implemented where applicable.
11. Authentication uses the cookie-based mechanism.
12. Authorization is permission-based.
13. Required persistence constraints exist.
14. The required UI is implemented.
15. SMS integration, where applicable, goes through the external SMS API boundary.
16. Invoice rendering is isolated behind an abstraction.
17. Financial snapshots are preserved.
18. Error handling is implemented.
19. Relevant logs and telemetry exist.
20. Automated tests cover important business rules.
21. No architectural boundary is bypassed.
22. No sensitive information leaks through logs, cookies, or the UI.
23. The implementation follows all mandatory business rules defined in this SRS.

## 17. Rules, Key Principles & Conclusion

### 17.1 Explicit Business Rules Summary

The following rules are mandatory and take precedence over any earlier conflicting requirements:

#### 17.1.1 Scope & Data

1. Category hierarchy must not exceed three levels.
2. Course slugs are unique.
3. Blog slugs are unique.
4. Phone numbers are unique when present.
5. Emails are unique when present.
6. Course attachments have count and size limits.
7. Attachment limits are configuration-driven.

#### 17.1.2 Deletion & Audit

1. Soft delete is the default deletion strategy.
2. Hard delete is allowed only for explicitly documented exceptions.
3. Soft-deletable entities must include `IsDeleted`, `DeletedAt`, and `DeletedBy` where applicable.
4. Business entities must include `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy` where applicable.
5. Ordinary queries must exclude soft-deleted records.
6. Financial records must not be physically deleted through ordinary administration.

#### 17.1.3 Authentication & Authorization

1. Passwords are never stored in plaintext.
2. Roles are dynamic.
3. Permissions are dynamic.
4. Roles receive permissions.
5. Authorization uses permissions, not hard-coded role names.
6. System roles/permissions are protected against malicious changes.
7. Browser authentication must be cookie-based.
8. JWT must not be the primary browser authentication mechanism.
9. Google authentication must establish a local, cookie-based authenticated session.
10. Google-authenticated users must provide a phone number before checkout.
11. Administrative operations must be protected by permissions.

#### 17.1.4 Pricing & Content Access

1. Course/product free status must be derived from a nullable `Price`.
2. `IsFree` must not be stored in the database.
3. Free episodes of paid courses are watchable without buying the whole course.
4. Paid non-free episodes require course ownership.
5. Public content must honor its display status.

#### 17.1.5 Cart, Order, Payment & Invoice

1. Carts are stored in Redis.
2. Order prices become snapshots.
3. Payment processing is idempotent.
4. Successful payment grants course access.
5. Real payment-provider integration is not required.
6. Invoice data must preserve historical financial snapshots.
7. Finalized invoices must not change through ordinary CRUD operations.
8. Invoice rendering must be isolated behind an abstraction.
9. The Domain layer must not reference reporting/PDF libraries.
10. Stimulsoft Reports.WEB is the current preferred invoice/reporting candidate.
11. QuestPDF and FastReport .NET must remain documented alternatives.

#### 17.1.6 SMS

1. Real SMS service integration is not required.
2. The fake SMS provider must be a separate API project.
3. The SMS API must use an in-memory data store (except SMS templates, which are stored durably).
4. The SMS API must expose an HTTP API like a real provider.
5. The SMS API must provide an HTML dashboard for inspecting SMS requests.
6. The fake SMS service must be replaceable with a real provider implementation.

#### 17.1.7 Architecture & Persistence

1. Repository abstractions (split for read and write) and Unit of Work must be used for persistence access.
2. Read and write repositories must be separated.
3. Read repositories must use Dapper.
4. Write repositories must use Entity Framework Core.
5. A generic repository must not be implemented.
6. `DbContext` must remain inside Infrastructure.
7. Controllers, Razor Pages, and Application handlers must not access `DbContext` directly.
8. Business logic belongs to the Application/Domain layers.

#### 17.1.8 Errors & Observability

1. Unexpected server errors must be logged and must not leak sensitive details; all sensitive information must be excluded from logs and authentication cookies.
2. Logs, traces, and metrics must be available through the observability infrastructure.
3. Grafana must provide unified observability dashboards.

### 17.2 Implementation Principles

The implementation must follow these principles:

#### 17.2.1 Architecture & Code

- Do not duplicate business rules between MVC, Razor Pages, and Application services.
- Do not expose `DbContext` outside Infrastructure.
- Use Unit of Work for consistent, persisted persistence boundaries.
- Prefer explicit Domain/Application rules over implicit behavior.
- Separate read and write persistence paths.
- Use Dapper for read-oriented repositories.
- Use EF Core for write-oriented repositories.
- Do not introduce a generic repository.
- Keep external integrations behind abstractions.
- Keep provider-specific implementation details behind abstractions.

#### 17.2.2 Security & Authentication

- Do not use ASP.NET Core Identity.
- Do not hard-code roles as the authorization mechanism.
- Do not hard-code permissions in presentation logic.
- Do not store passwords in plaintext.
- Do not expose internal exceptions to users.
- Do not render unsanitized HTML.
- Use secure cookie-based authentication for the browser.
- Keep configuration and secrets outside source code.
- Do not let audit fields be supplied by clients.

#### 17.2.3 Data & Deletion

- Do not store carts as the cart source of truth in PostgreSQL.
- Do not allow category depth beyond three levels.
- Prefer soft delete over physical deletion.
- Preserve audit information for administrative changes.
- Do not store a redundant `IsFree` field.
- Derive free status from the nullable `Price`.
- Preserve immutable financial snapshots in invoices.
- Do not physically delete historical financial records.

#### 17.2.4 Payment, SMS & Invoice

- Do not trust client-side prices.
- Make payment-result processing idempotent.
- Keep the SMS simulator outside the main application process.
- Treat the SMS simulator as an external HTTP dependency.
- Store SMS simulator state in memory.
- Provide a simple HTML dashboard for inspecting SMS traffic.
- Keep invoice rendering independent from the Domain layer.
- Keep reporting-library dependencies outside Domain and Application business models where possible.

### 17.3 Conclusion

This SRS defines the first implementation's scope of the online-course platform.

The system is deliberately designed around:

```text
Clean Architecture
        +
CQRS / MediatR
        +
Repository / Unit of Work
        +
Result Pattern
        +
PostgreSQL
        +
Redis
        +
Custom permission-based authorization
        +
Serilog / OpenTelemetry
        +
Loki / Tempo / Prometheus
        +
Grafana
```

The system must remain extendable without introducing unnecessary features outside the defined scope. Dynamic roles and permissions are a core requirement, while payment and SMS providers are deliberately abstracted so real integrations can later be added without changing the core business model.
