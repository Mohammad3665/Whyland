// Frequently used namespaces for the Application layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks)
// must not be repeated in individual files either.

#region External pipeline

global using MediatR;                               // commands, handlers and pipeline behaviors

global using FluentValidation;                      // rule builders and validator DI

global using Mapster;                               // request -> entity/response mapping

#endregion

#region Framework plumbing

global using System.Reflection;                     // property discovery used by the shared extensions

global using Microsoft.Extensions.DependencyInjection; // DI extension methods

global using Microsoft.Extensions.Logging;          // logger injection in behaviors and handlers

global using Microsoft.Extensions.Options;          // IOptions<T> for this layer's options classes

#endregion

#region Shared kernel

global using Whyland.Shared.Results;                // Result / Result<T> and the failure factories

global using Whyland.Shared.Filters;                // QueryRequest and the filter/sort contracts

global using Whyland.Shared.Enums;                  // FilterOperator, SortType, ...

global using Whyland.Shared.Errors;                 // Error / ErrorType transport shape

#endregion

#region Contracts this layer maps to and from

global using Whyland.Shared.Dtos.Content;           // content DTOs

global using Whyland.Shared.Dtos.Content.Faq;       // FAQ DTOs and filters

global using Whyland.Shared.Dtos.Catalog.Category;  // category listing DTOs

global using Whyland.Shared.Dtos.Identity;          // identity DTOs

global using Whyland.Shared.Dtos.Identity.User;     // user administration DTOs

#endregion

#region Domain-owned types and error codes

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
global using Whyland.Domain.Model.Authorization.Permission.Errors;        
global using Whyland.Domain.Model.Authorization.Role.Errors;              
global using Whyland.Domain.Model.Catalog.Category.Errors;                
global using Whyland.Domain.Model.Content.Faq.Errors;                     
global using Whyland.Domain.Model.Identity.ExternalLogin.Errors;          
global using Whyland.Domain.Model.Identity.User.Errors;                   
global using Whyland.Domain.Model.Catalog.Course.Enums;                   
global using Whyland.Domain.Model.Commerce.Common.Enums;                  
global using Whyland.Domain.Model.Commerce.Coupon.Enums;                  
global using Whyland.Domain.Model.Commerce.Order.Enums;                   
global using Whyland.Domain.Model.Commerce.Payment.Enums;                 
global using Whyland.Domain.Model.Identity.OtpRequest.Enums;              
global using Whyland.Domain.Model.Identity.UserProfile.Enums;             
global using Whyland.Domain.Model.Learning.UserEpisodeProgress.Enums;     
global using Whyland.Domain.Model.Settings.SiteSetting.Enums;             
global using Whyland.Domain.Model.Authorization.Common.Constants;

global using Whyland.Domain.Common.Errors;          // DomainError base contract

#endregion

#region Application abstractions

global using Whyland.Application.Common.Abstractions.CQRS.Command; // command + handler contracts

global using Whyland.Application.Common.Abstractions.CQRS.Query; // query + handler contracts

global using Whyland.Application.Common.Abstractions.UnitOfWork; // unit-of-work contract

global using Whyland.Application.Common.Interfaces.Services; // cross-cutting service ports

global using Whyland.Application.Common.Options;    // layer options classes

global using Whyland.Application.Common.Validation; // shared validators and rule extensions

#endregion
