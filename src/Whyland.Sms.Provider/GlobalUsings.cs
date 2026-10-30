// Frequently used namespaces for the Sms.Provider layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks)
// must not be repeated in individual files either.

#region Http plumbing

global using Refit;                                 // declarative HTTP interface

global using System.Text.Json;                      // [JsonPropertyName] on the payloads

#endregion

#region Provider parts

global using Whyland.Sms.Provider.Contracts;        // shared request/response records

global using Whyland.Sms.Provider.Abstractions;     // the client abstraction

#endregion
