namespace FedePortfolio.Web.Models;

public sealed class ExperienceItem
{
    public required string Company { get; init; }
    public required string Role { get; init; }
    public required string Period { get; init; }
    public required string Description { get; init; }
    public string? Stack { get; init; }
    public int SortOrder { get; init; }
}
