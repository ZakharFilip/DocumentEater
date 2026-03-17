using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 6: Извлекает Кол-во (сроки) из блоков нормативов/SLA, привязывает к приоритету или типу заявки.
/// </summary>
public static class QuantityParser
{
    private static readonly string[] SectionMarkers = new[]
    {
        "Нормативные сроки выполнения",
        "SLA",
        "Максимально допустимый срок",
        "Максимальное время",
        "нормативные сроки",
        "срок выполнения",
        "время выполнения",
        "реакция",
        "норматив"
    };

    // Число + (рабочих часов/дней) или просто число + часов/дней
    private static readonly Regex NumberWithUnits = new(
        @"(\d+)\s*\(\s*[^)]*\s*\)\s*(рабочих?\s*(?:часов|дней)|часов|дней)|(\d+)\s*(рабочих?\s*(?:часов|дней)|часов|дней)",
        RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.QuantitiesByKey.Clear();
        var text = ctx.AllText;
        string? currentKey = null;

        for (var i = 0; i < text.Count; i++)
        {
            var line = text[i];
            if (SectionMarkers.Any(m => line.Contains(m, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var p in ctx.Priorities)
                {
                    if (line.Contains(p, StringComparison.OrdinalIgnoreCase)) { currentKey = p; break; }
                }
                foreach (var b in ctx.BidTypes)
                {
                    if (b != "—" && line.Contains(b, StringComparison.OrdinalIgnoreCase)) { currentKey = b; break; }
                }
            }
            var m = NumberWithUnits.Match(line);
            if (m.Success)
            {
                var value = m.Value.Trim();
                if (string.IsNullOrEmpty(value)) continue;
                // Привязка к приоритету/типу: с той же строки или текущий контекст раздела
                var key = currentKey;
                if (key == null)
                {
                    foreach (var p in ctx.Priorities)
                    {
                        if (line.Contains(p, StringComparison.OrdinalIgnoreCase)) { key = p; break; }
                    }
                    if (key == null)
                    {
                        foreach (var b in ctx.BidTypes)
                        {
                            if (b != "—" && line.Contains(b, StringComparison.OrdinalIgnoreCase)) { key = b; break; }
                        }
                    }
                }
                key ??= "—";
                if (!ctx.QuantitiesByKey.ContainsKey(key))
                    ctx.QuantitiesByKey[key] = value;
            }
        }

        // Таблицы AllTables: сроки в строках с приоритетом или типом заявки
        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                var rowText = string.Join(" ", row);
                var match = NumberWithUnits.Match(rowText);
                if (!match.Success) continue;
                var value = match.Value.Trim();
                if (string.IsNullOrEmpty(value)) continue;
                var key = (string?)null;
                foreach (var p in ctx.Priorities)
                {
                    if (rowText.Contains(p, StringComparison.OrdinalIgnoreCase))
                    {
                        key = p;
                        break;
                    }
                }
                if (key == null)
                {
                    foreach (var b in ctx.BidTypes)
                    {
                        if (b != "—" && rowText.Contains(b, StringComparison.OrdinalIgnoreCase))
                        {
                            key = b;
                            break;
                        }
                    }
                }
                key ??= "—";
                if (!ctx.QuantitiesByKey.ContainsKey(key))
                    ctx.QuantitiesByKey[key] = value;
            }
        }
    }
}
