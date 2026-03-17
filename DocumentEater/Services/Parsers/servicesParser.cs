using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 10: Собирает список Услуг из Приложений №2 и №3 (allText) и из таблиц AllTables (нумерованные пункты, перечни).
/// </summary>
public static class ServicesParser
{
    // Нумерованные пункты: 1.1, 1.2, 2.1 или 1), 2) в начале строки
    private static readonly Regex NumberedItem = new(@"^\s*(\d+(?:\.\d+)*)\s*[.)]\s*(.+)$", RegexOptions.Multiline);
    private static readonly Regex NumberedItemLoose = new(@"(\d+(?:\.\d+)*)\s*[.)]\s*([^\r\n]+)", RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.Services.Clear();
        var blocks = ctx.AppendixBlocks;
        var text = ctx.AllText;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddService(string num, string content)
        {
            var key = num + " " + content;
            if (content.Length > 2 && seen.Add(key))
                ctx.Services.Add($"{num}. {content}");
        }

        foreach (var blockName in new[] { "Приложение2", "Приложение3" })
        {
            if (!blocks.TryGetValue(blockName, out var range))
                continue;
            if (range.Start >= text.Count) continue;
            var end = Math.Min(range.End, text.Count - 1);
            for (var i = range.Start; i <= end; i++)
            {
                var line = text[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                var m = NumberedItem.Match(line);
                if (m.Success)
                    AddService(m.Groups[1].Value, m.Groups[2].Value.Trim());
            }
        }

        // Поиск нумерованных пунктов по всему тексту (если в приложениях мало нашли)
        if (ctx.Services.Count == 0)
        {
            var fullText = string.Join("\n", text);
            foreach (Match m in NumberedItemLoose.Matches(fullText))
                AddService(m.Groups[1].Value, m.Groups[2].Value.Trim());
        }

        // Поиск в таблицах AllTables — каждая ячейка может содержать "1.1 Текст услуги"
        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                foreach (var cell in row)
                {
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    foreach (Match m in NumberedItemLoose.Matches(cell))
                        AddService(m.Groups[1].Value, m.Groups[2].Value.Trim());
                }
            }
        }

        if (ctx.Services.Count == 0)
            ctx.Services.Add("—");
    }
}
