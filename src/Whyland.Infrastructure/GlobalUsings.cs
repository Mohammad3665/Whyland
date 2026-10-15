// Frequently used namespaces for the Infrastructure layer, applied project-wide.

global using Microsoft.EntityFrameworkCore;                       // DbContext, query extensions
global using Microsoft.EntityFrameworkCore.Metadata.Builders;    // IEntityTypeConfiguration<T>
global using System.Reflection;

global using Whyland.Infrastructure.Persistence.Context;        // WhylandDbContext, SchemaConstants
global using Whyland.Application.Common.Abstractions.UnitOfWork; // Unit of Work contracts

global using Whyland.Domain.Common.Enums;
global using Whyland.Domain.Common.Stores;                       // StaticDataStore

// Entity groups referenced by configurations, seeders and services
global using Whyland.Domain.Entities.Authorization;
global using Whyland.Domain.Entities.Blog;
global using Whyland.Domain.Entities.Catalog;
global using Whyland.Domain.Entities.Commerce;
global using Whyland.Domain.Entities.Identity;
global using Whyland.Domain.Entities.Learning;
