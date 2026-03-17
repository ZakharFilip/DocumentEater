using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 3: Извлекает График из Приложений №2/4/6, при необходимости — из всего текста и таблиц (AllTables).
/// </summary>
public static class WorkSheduleParser
{
    private static readonly string[] KeyPhrases = new[]
    {
        "Календарь обслуживания",
        "Рабочее время",
        "Дни и часы проведения работ",
        "Выполнение Заявок",
        "график",
        "часы работы",
        "режим работы"
    };

    // Расширенные шаблоны: 24/7, 13/7 (с 9:00 до 22:00), 9:00-18:00, с 9 до 18, по будням, рабочие дни
    private static readonly Regex HoursPattern = new(
        @"(\d{1,2})/\s*7\s*\([^)]*\)|24/7|(\d{1,2}:\d{2})\s*[-–]\s*(\d{1,2}:\d{2})|(\d{1,2})\s*[-–]\s*(\d{1,2})\s*(?:часов|ч\.?)?|по будням\s*\d{1,2}:\d{2}-\d{1,2}:\d{2}|рабочи(й|е)\s*день|пн\s*[-–]\s*пт",
        RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.WorkSchedule = "—";
        var text = ctx.AllText;
        var blocks = ctx.AppendixBlocks;
        var appendicesToSearch = new[] { "Приложение2", "Приложение4", "Приложение6" };

        // 1) Поиск в блоках приложений по ключевым фразам
        foreach (var blockName in appendicesToSearch)
        {
            if (!blocks.TryGetValue(blockName, out var range))
                continue;
            if (range.Start >= text.Count)
                continue;
            var end = Math.Min(range.End, text.Count - 1);
            var inBlock = false;
            for (var i = range.Start; i <= end; i++)
            {
                var line = text[i];
                if (KeyPhrases.Any(p => line.Contains(p, StringComparison.OrdinalIgnoreCase)))
                    inBlock = true;
                if (!inBlock) continue;
                var m = HoursPattern.Match(line);
                if (m.Success)
                {
                    ctx.WorkSchedule = NormalizeSchedule(line.Trim());
                    return;
                }
            }
        }

        // 2) Поиск по всему тексту (если в приложениях не нашли)
        foreach (var line in text)
        {
            var m = HoursPattern.Match(line);
            if (m.Success)
            {
                ctx.WorkSchedule = NormalizeSchedule(line.Trim());
                return;
            }
        }

        // 3) Поиск в таблицах AllTables
        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                foreach (var cell in row)
                {
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    var m = HoursPattern.Match(cell);
                    if (m.Success)
                    {
                        ctx.WorkSchedule = NormalizeSchedule(cell.Trim());
                        return;
                    }
                }
            }
        }
    }

    private static string NormalizeSchedule(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "—";
        if (raw.Contains("24/7", StringComparison.OrdinalIgnoreCase))
            return "Ежедневно круглосуточно";
        var match = Regex.Match(raw, @"(\d{1,2})/\s*7\s*\([^)]*с\s*(\d{1,2}:\d{2})\s*до\s*(\d{1,2}:\d{2})[^)]*\)", RegexOptions.IgnoreCase);
        if (match.Success)
            return $"Ежедневно с {match.Groups[2].Value} до {match.Groups[3].Value}";
        match = Regex.Match(raw, @"(\d{1,2}:\d{2})\s*[-–]\s*(\d{1,2}:\d{2})", RegexOptions.IgnoreCase);
        if (match.Success)
            return $"Ежедневно с {match.Groups[1].Value} до {match.Groups[2].Value}";
        return raw;
    }
}
