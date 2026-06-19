using System.Globalization;

namespace FedePortfolio.Web.Models;

public sealed class Note
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required DateOnly Date { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public required string HtmlContent { get; init; }
    public required string RawContent { get; init; }

    public string FormattedDate => Date.ToString("d MMM yyyy", new CultureInfo("es-AR"));
    public string IsoDate => Date.ToString("yyyy-MM-dd");
}
