using System.Text.RegularExpressions;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 4: Определяет все типы заявок по заголовкам в тексте и в таблицах (AllTables).
/// </summary>
public static class BidParser
{
    private static readonly Regex AfterCategory = new(@"категорией\s+([^.:\r\n]+)", RegexOptions.IgnoreCase);
    private static readonly Regex AfterNa = new(@"на\s+([^.:\r\n,]+)", RegexOptions.IgnoreCase);
    private static readonly Regex AfterRabot = new(@"работ[ыа-я]*\s*[:\s]*([^.:\r\n]+)", RegexOptions.IgnoreCase);
    private static readonly Regex ПереченьРабот = new(@"Перечень\s+(?:Абонентных|Дополнительных|)?\s*работ", RegexOptions.IgnoreCase);

    public static void Parse(ParserContext ctx)
    {
        ctx.BidTypes.Clear();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void ProcessLine(string line)
        {
            var t = line.Trim();
            if (string.IsNullOrEmpty(t)) return;

            if (AfterCategory.Match(t).Success)
            {
                var name = ExtractName(AfterCategory.Match(t).Groups[1].Value);
                if (!string.IsNullOrEmpty(name) && seen.Add(name)) ctx.BidTypes.Add(name);
            }
            if (AfterNa.IsMatch(t) && (t.Contains("Заявок", StringComparison.OrdinalIgnoreCase) || t.Contains("Выполнение", StringComparison.OrdinalIgnoreCase)))
            {
                var name = ExtractName(AfterNa.Match(t).Groups[1].Value);
                if (!string.IsNullOrEmpty(name) && seen.Add(name)) ctx.BidTypes.Add(name);
            }
            if (ПереченьРабот.IsMatch(t) && AfterRabot.IsMatch(t))
            {
                var name = ExtractName(AfterRabot.Match(t).Groups[1].Value);
                if (!string.IsNullOrEmpty(name) && seen.Add(name)) ctx.BidTypes.Add(name);
            }
            // Типы заявок по ключевым словам (в контексте заявок/работ/категории, чтобы не ловить «Календарь обслуживания»)
            var bidContext = t.Contains("заявк", StringComparison.OrdinalIgnoreCase) || t.Contains("категори", StringComparison.OrdinalIgnoreCase) ||
                             t.Contains("работ", StringComparison.OrdinalIgnoreCase) || t.Contains("тип", StringComparison.OrdinalIgnoreCase);
            if (t.Contains("Инцидент", StringComparison.OrdinalIgnoreCase) && seen.Add("Инцидент")) ctx.BidTypes.Add("Инцидент");
            if (t.Contains("Обслуживание", StringComparison.OrdinalIgnoreCase) && !t.Contains("Календарь", StringComparison.OrdinalIgnoreCase) && seen.Add("Обслуживание")) ctx.BidTypes.Add("Обслуживание");
            if (t.Contains("обслуживание", StringComparison.OrdinalIgnoreCase) && bidContext && !t.StartsWith("Перечень", StringComparison.OrdinalIgnoreCase) && seen.Add("Обслуживание")) ctx.BidTypes.Add("Обслуживание");
            if (t.Contains("изменение", StringComparison.OrdinalIgnoreCase) && seen.Add("Изменение")) ctx.BidTypes.Add("Изменение");
            if (t.Contains("Консультац", StringComparison.OrdinalIgnoreCase) && seen.Add("Консультации")) ctx.BidTypes.Add("Консультации");
            if (t.Contains("Гарантийный ремонт", StringComparison.OrdinalIgnoreCase) && seen.Add("Гарантийный ремонт")) ctx.BidTypes.Add("Гарантийный ремонт");
        }

        foreach (var line in ctx.AllText)
            ProcessLine(line);

        // Поиск в таблицах AllTables (заголовки и ячейки)
        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                foreach (var cell in row)
                {
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    ProcessLine(cell);
                }
            }
        }

        if (ctx.BidTypes.Count == 0)
            ctx.BidTypes.Add("—");
    }

    private static string ExtractName(string s)
    {
        var v = s.Trim().TrimEnd('.', ',', ':', ';');
        var end = v.IndexOfAny(new[] { '.', ',', ':', ';', '\r', '\n' });
        return end >= 0 ? v[..end].Trim() : v;
    }
}
