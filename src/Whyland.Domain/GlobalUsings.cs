// Frequently used namespaces for the Domain layer, applied project-wide.
// Each file must remain compilable when read on its own; only namespaces used
// by many files across every entity group belong here.

global using System.Text.Json.Serialization; // [JsonStringEnumConverter] on enums
global using Whyland.Domain.Common;

global using Whyland.Domain.Entities.Common; // BaseEntity and audit base classes
global using Whyland.Domain.Common.Enums;    // Cross-entity domain enums
