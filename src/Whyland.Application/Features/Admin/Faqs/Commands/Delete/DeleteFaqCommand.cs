using Whyland.Application.Common.Abstractions.CQRS.Command;

namespace Whyland.Application.Features.Admin.Faqs.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing FAQ by its identifier.
/// </summary>
/// <param name="Id">The identifier of the FAQ to soft-delete.</param>
public sealed record DeleteFaqCommand(long Id) : ICommand;