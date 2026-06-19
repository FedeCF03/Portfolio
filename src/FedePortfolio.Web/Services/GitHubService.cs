using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FedePortfolio.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FedePortfolio.Web.Services;

public sealed class GitHubService : IGitHubService
{
    private const string SearchUrl = "https://api.github.com/search/issues";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly SiteMetadata _meta;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(HttpClient http, IMemoryCache cache, SiteMetadata meta, ILogger<GitHubService> logger)
    {
        _http = http;
        _cache = cache;
        _meta = meta;
        _logger = logger;
        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = new Uri("https://api.github.com/");
        }
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "fede-portfolio");
        }
        if (!_http.DefaultRequestHeaders.Contains("Accept"))
        {
            _http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        }
    }

    public async Task<IReadOnlyList<PullRequest>> GetMyPullRequestsAsync(int count = 10, CancellationToken ct = default)
    {
        var key = $"prs:own:{count}";
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var q = $"author:{_meta.GitHubUsername} type:pr is:public is:merged";
            var fetched = await FetchAsync(q, Math.Max(count * 2, 30), ct);
            return (IReadOnlyList<PullRequest>)fetched.Take(count).ToList();
        }) ?? Array.Empty<PullRequest>();
    }

    public async Task<IReadOnlyList<PullRequest>> GetOssContributionsAsync(int count = 10, CancellationToken ct = default)
    {
        var key = $"prs:oss:{count}";
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var q = $"author:{_meta.GitHubUsername} type:pr is:public is:merged";
            var fetched = await FetchAsync(q, Math.Max(count * 3, 30), ct);
            var ownUser = _meta.GitHubUsername;
            return (IReadOnlyList<PullRequest>)fetched
                .Where(p => !p.RepositoryFullName.StartsWith(ownUser + "/", StringComparison.OrdinalIgnoreCase))
                .Take(count)
                .ToList();
        }) ?? Array.Empty<PullRequest>();
    }

    private async Task<IReadOnlyList<PullRequest>> FetchAsync(string query, int count, CancellationToken ct)
    {
        try
        {
            var url = $"{SearchUrl}?q={Uri.EscapeDataString(query)}&sort=created&order=desc&per_page={count}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("GitHub API returned {Status} for query {Query}: {Body}",
                    (int)response.StatusCode, query, body);
                return Array.Empty<PullRequest>();
            }
            var result = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken: ct);
            if (result?.Items is null) return Array.Empty<PullRequest>();
            return result.Items.Select(i => new PullRequest
            {
                Title = i.Title ?? string.Empty,
                Url = i.HtmlUrl ?? string.Empty,
                RepositoryFullName = ExtractRepoFullName(i.RepositoryUrl),
                Number = i.Number,
                CreatedAt = i.CreatedAt,
                State = i.State
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub API request failed for query {Query}", query);
            return Array.Empty<PullRequest>();
        }
    }

    private static string ExtractRepoFullName(string? repositoryUrl)
    {
        if (string.IsNullOrEmpty(repositoryUrl)) return string.Empty;
        var parts = repositoryUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return repositoryUrl;
        return $"{parts[^2]}/{parts[^1]}";
    }

    private sealed class SearchResponse
    {
        [JsonPropertyName("items")]
        public List<SearchItem>? Items { get; set; }
    }

    private sealed class SearchItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("repository_url")]
        public string? RepositoryUrl { get; set; }
    }
}
