// Frequently used namespaces for the Domain layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks)
// must not be repeated in individual files either.

#region Serialization

global using System.Text.Json.Serialization;        // [JsonStringEnumConverter] on enums

#endregion

#region Domain kernel

global using Whyland.Domain.Common;                 // base contracts of the layer

global using Whyland.Domain.Common.Primitives;      // identity + soft-delete interfaces every entity implements

global using Whyland.Domain.Common.Entities;         // BaseEntity<T> and the audit base classes

#endregion

#region Shared kernel

global using Whyland.Shared.Errors;                 // Error / ErrorType carried by DomainError

#endregion

#region Aggregate namespaces referenced across aggregates

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



#endregion

global using Whyland.Domain.Abstractions;