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

global using Whyland.Shared.Filters;
global using Whyland.Shared.Results;

// Merged additions (frequent usings found by codebase analysis):
global using Microsoft.AspNetCore.Authorization;                  // permission attributes on repositories
global using Microsoft.Extensions.Configuration;                  // connection string readers
global using Microsoft.Extensions.DependencyInjection;            // DI extension methods
global using Npgsql;                                              // Dapper connection plumbing
global using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;      // PostgreSQL-specific model metadata
global using Microsoft.EntityFrameworkCore.Infrastructure;       // plugin/configuration services
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // seed value converters
global using Whyland.Application.Common.Interfaces.Identity;     // identity service contracts
global using Whyland.Domain.IRepositories.Content.Faq;           // Faq repository contracts
