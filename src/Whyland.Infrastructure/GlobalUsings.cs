// Frequently used namespaces for the Infrastructure layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks)
// must not be repeated in individual files either.

#region Framework plumbing

global using System.Reflection;                     // attribute discovery in the persistence helpers

global using Microsoft.Extensions.Configuration;    // connection string + option readers

global using Microsoft.Extensions.DependencyInjection; // DI extension methods

global using Microsoft.Extensions.Logging;          // logger injection in services and repositories

global using Microsoft.AspNetCore.Authorization;    // permission attributes on repositories

#endregion

#region Data access

global using Microsoft.EntityFrameworkCore;         // DbContext and query extensions

global using Microsoft.EntityFrameworkCore.Metadata.Builders; // IEntityTypeConfiguration<T>

global using Microsoft.EntityFrameworkCore.Infrastructure; // plugin/configuration services

global using Microsoft.EntityFrameworkCore.Migrations; // generated migration scaffolding

global using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // value converters used by seeding

global using Npgsql;                                // Dapper connection plumbing

global using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata; // PostgreSQL-specific model metadata

global using Dapper;                                // query and command execution

#endregion

#region Persistence plumbing

global using Whyland.Infrastructure.Persistence.Context; // WhylandDbContext, SchemaConstants

global using Whyland.Infrastructure.Persistence.Extensions; // reusable persistence helpers

#endregion

#region Application ports implemented here

global using Whyland.Application.Common.Abstractions.UnitOfWork; // unit-of-work contract

global using Whyland.Application.Common.Interfaces.Identity; // identity service contracts

global using Whyland.Application.Common.Interfaces.Services; // cross-cutting service ports

#endregion

#region Domain + shared types

global using Whyland.Domain.Abstractions;          // identifier contracts and the unwrap helper

global using Whyland.Domain.Model.Authorization.Permission.Identifiers;

global using Whyland.Domain.Model.Authorization.Role.Identifiers;

global using Whyland.Domain.Model.Blog.BlogCategory.Identifiers;

global using Whyland.Domain.Model.Blog.BlogPost.Identifiers;

global using Whyland.Domain.Model.Catalog.Category.Identifiers;

global using Whyland.Domain.Model.Catalog.Course.Identifiers;

global using Whyland.Domain.Model.Catalog.CourseFaq.Identifiers;

global using Whyland.Domain.Model.Catalog.CourseSection.Identifiers;

global using Whyland.Domain.Model.Catalog.Episode.Identifiers;

global using Whyland.Domain.Model.Catalog.EpisodeAttachment.Identifiers;

global using Whyland.Domain.Model.Commerce.Coupon.Identifiers;

global using Whyland.Domain.Model.Commerce.Invoice.Identifiers;

global using Whyland.Domain.Model.Commerce.InvoiceItem.Identifiers;

global using Whyland.Domain.Model.Commerce.Order.Identifiers;

global using Whyland.Domain.Model.Commerce.OrderItem.Identifiers;

global using Whyland.Domain.Model.Commerce.Payment.Identifiers;

global using Whyland.Domain.Model.Content.Faq.Identifiers;

global using Whyland.Domain.Model.Identity.ExternalLogin.Identifiers;

global using Whyland.Domain.Model.Identity.InstructorProfile.Identifiers;

global using Whyland.Domain.Model.Identity.OtpRequest.Identifiers;

global using Whyland.Domain.Model.Identity.User.Identifiers;

global using Whyland.Domain.Model.Identity.UserProfile.Identifiers;

global using Whyland.Domain.Model.Settings.SiteSetting.Identifiers;

global using Whyland.Domain.Model.Authorization.Permission.Entities;      
global using Whyland.Domain.Model.Authorization.Role.Entities;            
global using Whyland.Domain.Model.Authorization.RolePermission.Entities;  
global using Whyland.Domain.Model.Authorization.UserRole.Entities;        
global using Whyland.Domain.Model.Blog.BlogCategory.Entities;             
global using Whyland.Domain.Model.Blog.BlogPost.Entities;                 
global using Whyland.Domain.Model.Catalog.Category.Entities;              
global using Whyland.Domain.Model.Catalog.Course.Entities;                
global using Whyland.Domain.Model.Catalog.CourseCategory.Entities;        
global using Whyland.Domain.Model.Catalog.CourseFaq.Entities;             
global using Whyland.Domain.Model.Catalog.CourseInstructor.Entities;      
global using Whyland.Domain.Model.Catalog.CourseSection.Entities;         
global using Whyland.Domain.Model.Catalog.Episode.Entities;               
global using Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities;     
global using Whyland.Domain.Model.Commerce.Coupon.Entities;               
global using Whyland.Domain.Model.Commerce.Invoice.Entities;              
global using Whyland.Domain.Model.Commerce.InvoiceItem.Entities;          
global using Whyland.Domain.Model.Commerce.Order.Entities;                
global using Whyland.Domain.Model.Commerce.OrderItem.Entities;            
global using Whyland.Domain.Model.Commerce.Payment.Entities;              
global using Whyland.Domain.Model.Content.Faq.Entities;                   
global using Whyland.Domain.Model.Identity.ExternalLogin.Entities;        
global using Whyland.Domain.Model.Identity.InstructorProfile.Entities;    
global using Whyland.Domain.Model.Identity.OtpRequest.Entities;           
global using Whyland.Domain.Model.Identity.User.Entities;                 
global using Whyland.Domain.Model.Identity.UserProfile.Entities;          
global using Whyland.Domain.Model.Interactions.FavoriteCourse.Entities;   
global using Whyland.Domain.Model.Learning.UserCourse.Entities;           
global using Whyland.Domain.Model.Learning.UserEpisodeProgress.Entities;  
global using Whyland.Domain.Model.Settings.SiteSetting.Entities;          
global using Whyland.Domain.Model.Catalog.Course.Enums;                   
global using Whyland.Domain.Model.Commerce.Common.Enums;                  
global using Whyland.Domain.Model.Commerce.Coupon.Enums;                  
global using Whyland.Domain.Model.Commerce.Order.Enums;                   
global using Whyland.Domain.Model.Commerce.Payment.Enums;                 
global using Whyland.Domain.Model.Identity.OtpRequest.Enums;              
global using Whyland.Domain.Model.Identity.UserProfile.Enums;             
global using Whyland.Domain.Model.Learning.UserEpisodeProgress.Enums;     
global using Whyland.Domain.Model.Settings.SiteSetting.Enums;             
global using Whyland.Domain.Model.Authorization.Role.Repositories;
global using Whyland.Domain.Model.Catalog.Category.Repositories;          
global using Whyland.Domain.Model.Catalog.CourseFaq.Repositories;
global using Whyland.Domain.Model.Catalog.CourseSection.Repositories;
global using Whyland.Domain.Model.Content.Faq.Repositories;               
global using Whyland.Domain.Model.Identity.ExternalLogin.Repositories;    
global using Whyland.Domain.Model.Identity.InstructorProfile.Repositories;
global using Whyland.Domain.Model.Identity.OtpRequest.Repositories;       
global using Whyland.Domain.Model.Identity.User.Repositories;             
global using Whyland.Domain.Model.Authorization.Common.Constants;

global using Whyland.Domain.Common.Stores;          // StaticDataStore

global using Whyland.Shared.Enums;                  // FilterOperator, SortType, ...

global using Whyland.Shared.Filters;                // query contracts used by Dapper readers

global using Whyland.Shared.Results;                // Result / Result<T>

#endregion
