using FedePortfolio.Web.Models;

namespace FedePortfolio.Web.Services;

public sealed class SiteMetadata
{
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string Location { get; init; }
    public required string Tagline { get; init; }
    public required string HeroDescription { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyList<Project> Projects { get; init; }
    public required IReadOnlyList<ExperienceItem> Experiences { get; init; }
    public required IReadOnlyList<SocialLink> Socials { get; init; }
    public string SiteVersion { get; init; } = "1.0.0";
    public string? CommitSha { get; init; }
    public DateTimeOffset? LastModified { get; init; }

    public string? CommitShort => CommitSha is { Length: >= 7 } sha ? sha[..7] : CommitSha;
    public string FormattedLastModified => LastModified?.ToString("MMM d, yyyy") ?? string.Empty;
}
