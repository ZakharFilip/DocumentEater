namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 9: Извлекает ЗИП-И, ЗИП-П, ЗИП-З из таблицы "Правила установки комплектов ЗИП" или блоков с ЗИП.
/// </summary>
public static class ZipParser
{
    public static void Parse(ParserContext ctx)
    {
        ctx.ZipI = "—";
        ctx.ZipP = "—";
        ctx.ZipZ = "—";

        foreach (var table in ctx.AllTables)
        {
            var headerRow = table.Count > 0 ? string.Join(" ", table[0]) : "";
            if (!headerRow.Contains("ЗИП", StringComparison.OrdinalIgnoreCase) &&
                !headerRow.Contains("Правила установки", StringComparison.OrdinalIgnoreCase) &&
                !headerRow.Contains("комплект", StringComparison.OrdinalIgnoreCase))
                continue;

            var colZ = FindColumnIndex(table[0], "ЗИП-З", "ЗИП-з", "ЗИП З");
            var colI = FindColumnIndex(table[0], "ЗИП-И", "ЗИП-и", "ЗИП И");
            var colP = FindColumnIndex(table[0], "ЗИП-П", "ЗИП-п", "ЗИП П");

            for (var r = 1; r < table.Count; r++)
            {
                var row = table[r];
                if (colZ >= 0 && colZ < row.Count && !string.IsNullOrWhiteSpace(row[colZ]))
                    ctx.ZipZ = row[colZ].Trim();
                if (colI >= 0 && colI < row.Count && !string.IsNullOrWhiteSpace(row[colI]))
                    ctx.ZipI = row[colI].Trim();
                if (colP >= 0 && colP < row.Count && !string.IsNullOrWhiteSpace(row[colP]))
                    ctx.ZipP = row[colP].Trim();
            }
            if (ctx.ZipZ != "—" || ctx.ZipI != "—" || ctx.ZipP != "—")
                return;
        }

        var fullText = string.Join(" ", ctx.AllText);
        if (fullText.Contains("ЗИП-И", StringComparison.OrdinalIgnoreCase)) ctx.ZipI = "см. текст";
        if (fullText.Contains("ЗИП-П", StringComparison.OrdinalIgnoreCase)) ctx.ZipP = "см. текст";
        if (fullText.Contains("ЗИП-З", StringComparison.OrdinalIgnoreCase)) ctx.ZipZ = "см. текст";
    }

    private static int FindColumnIndex(List<string> headerRow, params string[] names)
    {
        for (var i = 0; i < headerRow.Count; i++)
        {
            var cell = headerRow[i].Trim();
            if (names.Any(n => cell.Contains(n, StringComparison.OrdinalIgnoreCase)))
                return i;
        }
        return -1;
    }
}
