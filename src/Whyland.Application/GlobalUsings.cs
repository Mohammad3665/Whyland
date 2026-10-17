// Frequently used namespaces for the Application layer, applied project-wide.

global using System.Reflection;              // Property discovery for filter/sort extensions
global using MediatR;                        // Pipeline behaviors
global using FluentValidation;               // Validators and DI extensions

global using Whyland.Shared.Results;  // Result / Result<T> abstractions
global using Whyland.Shared.Filters;  // QueryRequest and filter contracts
global using Whyland.Shared.Enums;    // FilterOperator, SortType, ...
global using Whyland.Shared.Errors;

// Merged additions (frequent usings found by codebase analysis):
global using Mapster;                                              // request→entity/response mapping
global using Microsoft.Extensions.DependencyInjection;             // DI extension methods
global using Whyland.Domain.Common.Errors;                         // DomainError contracts
global using Whyland.Shared.Dtos.Content;
global using Whyland.Application.Common.Abstractions.CQRS.Command; // command pipeline abstractions
global using Whyland.Application.Common.Abstractions.CQRS.Query;   // query pipeline abstractions
global using Whyland.Application.Common.Abstractions.UnitOfWork;   // unit-of-work contracts
global using Whyland.Application.Common.Validation;                // shared validators
global using Whyland.Application.Features.Admin.Faqs.Errors;       // FaqErrors contracts
