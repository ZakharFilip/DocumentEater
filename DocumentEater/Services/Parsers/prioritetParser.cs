using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 5: Определяет все приоритеты (Высокий, Средний, Низкий, Критический, по таблицам нормативов).
/// </summary>
public static class PrioritetParser
{
    private static readonly string[] PriorityWords = new[]
    {
        "Критический", "Высокий", "Средний", "Низкий", "Срочный", "Плановый"
    };

    private static readonly Regex PriorityWithNumber = new(@"приоритет\s*[:\s]*(\d+|\w+)", RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.Priorities.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var text = ctx.AllText;

        foreach (var line in text)
        {
            foreach (var word in PriorityWords)
            {
                if (line.Contains(word, StringComparison.OrdinalIgnoreCase) && seen.Add(word))
                    ctx.Priorities.Add(word);
            }
            var m = PriorityWithNumber.Match(line);
            if (m.Success)
            {
                var v = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(v) && seen.Add(v))
                    ctx.Priorities.Add(v);
            }
        }

        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                foreach (var cell in row)
                {
                    foreach (var word in PriorityWords)
                    {
                        if (cell.Contains(word, StringComparison.OrdinalIgnoreCase) && seen.Add(word))
                            ctx.Priorities.Add(word);
                    }
                }
            }
        }

        if (ctx.Priorities.Count == 0)
            ctx.Priorities.Add("—");
    }
}
