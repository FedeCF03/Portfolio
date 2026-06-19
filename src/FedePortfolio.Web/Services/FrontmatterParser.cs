using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FedePortfolio.Web.Services;

internal static class FrontmatterParser
{
    public sealed record Meta(string? Title, DateOnly? Date, IReadOnlyList<string>? Tags);

    public static (Meta Meta, string Body) Parse(string raw)
    {
        if (!raw.StartsWith("---"))
        {
            return (new Meta(null, null, null), raw);
        }

        var lines = raw.Replace("\r\n", "\n").Split('\n');
        if (lines.Length < 3 || lines[0].Trim() != "---")
        {
            return (new Meta(null, null, null), raw);
        }

        var endIndex = -1;
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex < 0)
        {
            return (new Meta(null, null, null), raw);
        }

        var yaml = string.Join('\n', lines.Skip(1).Take(endIndex - 1));
        var body = string.Join('\n', lines.Skip(endIndex + 1)).TrimStart('\n');

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        Dictionary<string, object>? dict;
        try
        {
            dict = deserializer.Deserialize<Dictionary<string, object>>(yaml);
        }
        catch
        {
            return (new Meta(null, null, null), body);
        }

        string? title = dict.TryGetValue("title", out var t) ? t?.ToString() : null;
        DateOnly? date = null;
        if (dict.TryGetValue("date", out var d) && d is not null)
        {
            if (DateOnly.TryParse(d.ToString(), out var parsed)) date = parsed;
        }
        IReadOnlyList<string>? tags = null;
        if (dict.TryGetValue("tags", out var tg) && tg is not null)
        {
            if (tg is IEnumerable<object> list)
            {
                tags = list.Select(x => x.ToString() ?? string.Empty).Where(s => s.Length > 0).ToList();
            }
            else
            {
                var raw1 = tg.ToString() ?? string.Empty;
                tags = raw1.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        return (new Meta(title, date, tags), body);
    }
}
