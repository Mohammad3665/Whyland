// Frequently used namespaces for the Web layer, applied project-wide.
// Only namespaces that many files in this layer actually need belong here: a
// global using nothing consumes is dead code, and one that makes two same-named
// types collide costs more clarity than it saves.
// ImplicitUsings is enabled, so the SDK namespaces (System, System.Collections.Generic,
// System.IO, System.Linq, System.Net.Http, System.Threading, System.Threading.Tasks, Microsoft.AspNetCore.Builder/Hosting/Http, Microsoft.Extensions.Configuration/DI/Hosting/Logging, System.Net.Http.Json)
// must not be repeated in individual files either.

#region Framework plumbing

global using System.Reflection;                     // attribute discovery in the helpers

global using System.Security.Claims;                // ClaimsPrincipal extensions and claim types

global using Microsoft.AspNetCore.Mvc;              // controllers and attribute routing

global using Microsoft.AspNetCore.Mvc.RazorPages;   // page models

global using Microsoft.AspNetCore.Authorization;    // [Authorize] on the admin area

global using Serilog;                               // structural logging entry points

#endregion

#region Application entry points

global using Whyland.Application.Common.Abstractions.CQRS.Dispatchers; // the sender the endpoints call

global using Whyland.Application.Common.Interfaces.Identity; // signed-in user accessor

#endregion

#region Web helpers

global using Whyland.Web.Common.Extensions;         // middleware & DI extension methods

global using Whyland.Web.Common.Identity.Constants; // auth scheme + claim names

global using Whyland.Shared.Text;                   // Persian digit + text helpers

#endregion
