// Frequently used namespaces for the Web layer, applied project-wide.

global using Microsoft.AspNetCore.Mvc;             // Controllers and attribute routing
global using Microsoft.AspNetCore.Mvc.RazorPages;  // Page models
global using System.Reflection;
global using Serilog;                              // Structural logging entry points

// Merged additions (frequent usings found by codebase analysis):
global using Whyland.Web.Common.Extensions;      // middleware & DI extension methods
