// Frequently used namespaces for the Sms layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks, Microsoft.AspNetCore.Builder/Hosting/Http, Microsoft.Extensions.Configuration/DI/Hosting/Logging, System.Net.Http.Json)
// must not be repeated in individual files either.

#region Framework plumbing

global using System.Reflection;                     // attribute discovery in the storage layer

global using System.Text.Json;                      // payload serialization for the queue

global using Microsoft.AspNetCore.Mvc;              // controllers and attribute routing

global using Microsoft.AspNetCore.Authorization;    // [Authorize] on the panel and API endpoints

global using Microsoft.Extensions.Options;          // IOptions<T> for the SMS options

#endregion

#region Sms application parts

global using Whyland.Sms.Contracts.V1;              // the versioned public contract

global using Whyland.Sms.Storage;                   // queued message store

global using Whyland.Sms.Security;                  // token + cookie helpers

global using Whyland.Sms.ViewModels;                // page view models

#endregion
