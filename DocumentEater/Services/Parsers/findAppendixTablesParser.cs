using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 2: Находит все Приложения и таблицы в allText/allTables, сохраняет блоки в контекст.
/// </summary>
public static class FindAppendixTablesParser
{
    private static readonly Regex AppendixRegex = new(@"Приложение\s*№\s*(\d+)", RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.AppendixBlocks.Clear();
        var text = ctx.AllText;
        if (text.Count == 0) return;

        for (var i = 0; i < text.Count; i++)
        {
            var m = AppendixRegex.Match(text[i]);
            if (!m.Success) continue;
            var name = "Приложение" + m.Groups[1].Value;
            var start = i;
            var end = text.Count - 1;
            for (var j = i + 1; j < text.Count; j++)
            {
                if (AppendixRegex.IsMatch(text[j]))
                {
                    end = j - 1;
                    break;
                }
            }
            ctx.AppendixBlocks[name] = (start, end);
        }

        // Для таблиц: Start = индекс таблицы (0..n), End = число строк. Не использовать как диапазон строк allText.
        for (var t = 0; t < ctx.AllTables.Count; t++)
        {
            var tableName = "Таблица" + (t + 1);
            var rows = ctx.AllTables[t].Count;
            ctx.AppendixBlocks[tableName] = (t, rows);
        }
    }
}
