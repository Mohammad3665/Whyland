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

global using Whyland.Domain.Common.Enums;           // cross-entity domain enums

global using Whyland.Domain.Common.Primitives;      // identity + soft-delete interfaces every entity implements

global using Whyland.Domain.Entities.Common;        // BaseEntity<T> and the audit base classes

#endregion

#region Shared kernel

global using Whyland.Shared.Errors;                 // Error / ErrorType carried by DomainError

#endregion

#region Entity groups referenced across groups

global using Whyland.Domain.Entities.Catalog;       // course catalogue entities

global using Whyland.Domain.Entities.Identity;      // identity entities referenced by authorization

#endregion
