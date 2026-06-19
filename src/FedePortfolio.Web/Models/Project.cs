namespace FedePortfolio.Web.Models;

public sealed class Project
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string Description { get; init; }
    public string? Stack { get; init; }
}
