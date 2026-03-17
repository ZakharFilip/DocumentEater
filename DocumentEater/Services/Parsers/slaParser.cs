using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 8: Извлекает SLA и SLA повышенная нагрузка (таблица ЗИП, зона, выезд, 5 рабочих дней и т.д.).
/// </summary>
public static class SlaParser
{
    private static readonly string[] SlaHighLoadMarkers = new[]
    {
        "зона",
        "Выезд зона",
        "на следующий рабочий день",
        "5 рабочих дней",
        "при повышенной нагрузке"
    };

    public static void Parse(ParserContext ctx)
    {
        ctx.Sla = "—";
        ctx.SlaHighLoad = "—";
        var text = ctx.AllText;
        var fullText = string.Join(" ", text);

        foreach (var table in ctx.AllTables)
        {
            var tableText = string.Join(" ", table.SelectMany(r => r));
            if (tableText.Contains("ЗИП", StringComparison.OrdinalIgnoreCase) ||
                tableText.Contains("Правила установки комплектов", StringComparison.OrdinalIgnoreCase) ||
                tableText.Contains("ККТ", StringComparison.OrdinalIgnoreCase))
            {
                if (table.Count > 1 && table[0].Count > 1)
                    ctx.Sla = table[0].Count > 2 ? string.Join("; ", table[0].Skip(1).Take(2)) : string.Join(" ", table[0]);
                break;
            }
        }

        // Маркеры повышенной нагрузки — в тексте и в таблицах AllTables
        foreach (var marker in SlaHighLoadMarkers)
        {
            if (fullText.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                ctx.SlaHighLoad = "Да";
                break;
            }
        }
        if (ctx.SlaHighLoad == "—")
        {
            foreach (var table in ctx.AllTables)
            {
                var tableText = string.Join(" ", table.SelectMany(r => r));
                foreach (var marker in SlaHighLoadMarkers)
                {
                    if (tableText.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.SlaHighLoad = "Да";
                        break;
                    }
                }
                if (ctx.SlaHighLoad != "—") break;
            }
        }
        if (ctx.SlaHighLoad == "—" && fullText.Contains("Приложение № 6", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var t in ctx.AllTables)
            {
                var tt = string.Join(" ", t.SelectMany(r => r));
                if (tt.Contains("Зона", StringComparison.OrdinalIgnoreCase) || tt.Contains("Цена", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.SlaHighLoad = "Таблица прил. №6";
                    break;
                }
            }
        }
    }
}
