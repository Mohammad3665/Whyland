namespace Whyland.Shared.Dtos.Content.Faq;

/// <summary>Shape of a public FAQ shown on the site (no audit fields).</summary>
public sealed record FaqPublicItemDto(string Title, string Description);