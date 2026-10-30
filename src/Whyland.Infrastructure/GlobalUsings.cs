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

global using Whyland.Domain.IRepositories.Identity.User; // user repository contracts

#endregion

#region Domain + shared types

global using Whyland.Domain.Common.Enums;           // cross-entity domain enums

global using Whyland.Domain.Common.Stores;          // StaticDataStore

global using Whyland.Domain.Entities.Authorization; // role and permission entities

global using Whyland.Domain.Entities.Blog;          // blog entities

global using Whyland.Domain.Entities.Catalog;       // catalogue entities

global using Whyland.Domain.Entities.Commerce;      // commerce entities

global using Whyland.Domain.Entities.Identity;      // identity entities

global using Whyland.Domain.Entities.Learning;      // learning entities

global using Whyland.Domain.IRepositories.Content.Faq; // Faq repository contracts

global using Whyland.Shared.Enums;                  // FilterOperator, SortType, ...

global using Whyland.Shared.Filters;                // query contracts used by Dapper readers

global using Whyland.Shared.Results;                // Result / Result<T>

#endregion
