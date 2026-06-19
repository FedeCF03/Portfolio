using FedePortfolio.Web.Models;

namespace FedePortfolio.Web.Services;

public interface IGitHubService
{
    Task<IReadOnlyList<PullRequest>> GetMyPullRequestsAsync(int count = 10, CancellationToken ct = default);
    Task<IReadOnlyList<PullRequest>> GetOssContributionsAsync(int count = 10, CancellationToken ct = default);
}
