namespace FedePortfolio.Web.Models;

public sealed class SocialLink
{
    public required string Label { get; init; }
    public required string Handle { get; init; }
    public required string Url { get; init; }
    public string? Rel { get; init; }
}
