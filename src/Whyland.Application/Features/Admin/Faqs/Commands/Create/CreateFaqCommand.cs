using Whyland.Application.Common.Abstractions.CQRS.Command;

namespace Whyland.Application.Features.Admin.Faqs.Commands.Create;

/// <summary>
/// Command that creates a new FAQ from the supplied content and state,
/// returning the identifier of the created FAQ.
/// </summary>
/// <param name="Title">The question title of the FAQ.</param>
/// <param name="Description">The answer/description of the FAQ.</param>
/// <param name="DisplayOrder">The display order used for sorting FAQs.</param>
/// <param name="IsActive">Whether the FAQ is visible to end users.</param>
public sealed record CreateFaqCommand(string Title, string Description, int DisplayOrder, bool IsActive) : ICommand<long>;
