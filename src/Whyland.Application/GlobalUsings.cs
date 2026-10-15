// Frequently used namespaces for the Application layer, applied project-wide.

global using System.Reflection;              // Property discovery for filter/sort extensions
global using MediatR;                        // Pipeline behaviors
global using FluentValidation;               // Validators and DI extensions

global using Whyland.Domain.Common.Results;  // Result / Result<T> abstractions
global using Whyland.Domain.Common.Filters;  // QueryRequest and filter contracts
global using Whyland.Domain.Common.Enums;    // FilterOperator, SortType, ...
