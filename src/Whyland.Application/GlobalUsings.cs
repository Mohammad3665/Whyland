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

global using Whyland.Shared.Dtos.Identity;          // identity DTOs

global using Whyland.Shared.Dtos.Identity.User;     // user administration DTOs

#endregion

#region Domain-owned types and error codes

global using Whyland.Domain.Common.Errors;          // DomainError base contract

global using Whyland.Domain.Entities.Content;       // Faq and the other content entities

global using Whyland.Domain.Entities.Content.Errors; // FaqErrors code catalog

global using Whyland.Domain.Entities.Identity;      // User and the other identity entities

global using Whyland.Domain.Entities.Identity.Errors; // UserErrors code catalog

#endregion

#region Application abstractions

global using Whyland.Application.Common.Abstractions.CQRS.Command; // command + handler contracts

global using Whyland.Application.Common.Abstractions.CQRS.Query; // query + handler contracts

global using Whyland.Application.Common.Abstractions.UnitOfWork; // unit-of-work contract

global using Whyland.Application.Common.Interfaces.Services; // cross-cutting service ports

global using Whyland.Application.Common.Options;    // layer options classes

global using Whyland.Application.Common.Validation; // shared validators and rule extensions

#endregion
