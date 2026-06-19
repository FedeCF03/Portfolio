using FedePortfolio.Web.Models;

namespace FedePortfolio.Web.Services;

public interface INotesService
{
    IReadOnlyList<Note> GetAll();
    Note? GetBySlug(string slug);
}

public sealed class NotesService : INotesService
{
    private readonly MarkdownRenderer _renderer;
    private readonly ILogger<NotesService> _logger;
    private readonly string _contentRoot;
    private List<Note>? _cache;

    public NotesService(MarkdownRenderer renderer, IWebHostEnvironment env, ILogger<NotesService> logger)
    {
        _renderer = renderer;
        _logger = logger;
        _contentRoot = Path.Combine(env.ContentRootPath, "Content", "notes");
    }

    public IReadOnlyList<Note> GetAll()
    {
        if (_cache is not null) return _cache;
        Load();
        return _cache!;
    }

    public Note? GetBySlug(string slug) => GetAll().FirstOrDefault(n => n.Slug == slug);

    private void Load()
    {
        if (!Directory.Exists(_contentRoot))
        {
            _logger.LogWarning("Notes directory not found at {Path}", _contentRoot);
            _cache = new List<Note>();
            return;
        }

        var notes = new List<Note>();
        foreach (var file in Directory.EnumerateFiles(_contentRoot, "*.md", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var note = ParseFile(file);
                if (note is not null) notes.Add(note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse note {File}", file);
            }
        }

        _cache = notes.OrderByDescending(n => n.Date).ToList();
        _logger.LogInformation("Loaded {Count} notes from {Path}", _cache.Count, _contentRoot);
    }

    private Note? ParseFile(string path)
    {
        var raw = File.ReadAllText(path);
        var (meta, body) = FrontmatterParser.Parse(raw);
        var slug = Path.GetFileNameWithoutExtension(path);
        var title = meta.Title ?? SlugToTitle(slug);
        var date = meta.Date ?? ExtractDateFromFileName(slug) ?? DateOnly.FromDateTime(File.GetLastWriteTime(path));
        var tags = meta.Tags ?? Array.Empty<string>();
        var html = _renderer.Render(body);
        return new Note
        {
            Slug = slug,
            Title = title,
            Date = date,
            Tags = tags,
            HtmlContent = html,
            RawContent = body
        };
    }

    private static DateOnly? ExtractDateFromFileName(string slug)
    {
        var prefix = slug.Split('-', 4);
        if (prefix.Length >= 3 &&
            int.TryParse(prefix[0], out var y) &&
            int.TryParse(prefix[1], out var m) &&
            int.TryParse(prefix[2], out var d) &&
            y is >= 2000 and <= 2100 && m is >= 1 and <= 12 && d is >= 1 and <= 31)
        {
            return new DateOnly(y, m, d);
        }
        return null;
    }

    private static string SlugToTitle(string slug)
    {
        var stripped = ExtractDateFromFileName(slug) is null
            ? slug
            : string.Join('-', slug.Split('-', 4).Skip(3));
        return string.Join(' ', stripped.Split('-').Select(w => w.Length == 0 ? w : char.ToUpper(w[0]) + w[1..]));
    }
}
