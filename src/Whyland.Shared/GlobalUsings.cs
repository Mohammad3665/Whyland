// Frequently used namespaces for the Shared layer, applied project-wide.
// Namespaces referenced frequently across the codebase are hoisted here once,
// so individual files only need non-repeated imports.

global using System.Text.Json.Serialization; // [JsonPropertyName] / [JsonIgnore] attributes
