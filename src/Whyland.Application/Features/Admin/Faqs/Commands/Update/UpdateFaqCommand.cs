using Whyland.Application.Common.Abstractions.CQRS.Command;

namespace Whyland.Application.Features.Admin.Faqs.Commands.Update;

/// <summary>
/// Command that updates an existing FAQ with new content and state.
/// </summary>
/// <param name="Id">The identifier of the FAQ to update.</param>
/// <param name="Title">The new title of the FAQ.</param>
/// <param name="Description">The new answer/description of the FAQ.</param>
/// <param name="DisplayOrder">The new display order used for sorting FAQs.</param>
/// <param name="IsActive">Whether the FAQ is visible to end users.</param>
public sealed record UpdateFaqCommand(long Id, string Title, string Description, int DisplayOrder, bool IsActive) : ICommand;
