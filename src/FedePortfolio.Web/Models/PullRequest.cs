namespace FedePortfolio.Web.Models;

public sealed class PullRequest
{
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string RepositoryFullName { get; init; }
    public required int Number { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? State { get; init; }
}
