using System.Collections.Generic;

namespace DocumentEater.Services.Parsers;

/// <summary>
/// Главный парсер: запускает все пошаговые парсеры и собирает из контекста итоговую таблицу (listForTable).
/// </summary>
public static class GeneralParser
{
    /// <summary>
    /// Заполняет контекст, запуская все парсеры по порядку, затем формирует список строк таблицы.
    /// Порядок колонок: TableColumns.Headers.
    /// </summary>
    public static List<List<string>> ParseAndBuildTable(ParserContext ctx)
    {
        FindAppendixTablesParser.Parse(ctx);
        WorkSheduleParser.Parse(ctx);
        BidParser.Parse(ctx);
        PrioritetParser.Parse(ctx);
        QuantityParser.Parse(ctx);
        ConditionParser.Parse(ctx);
        SlaParser.Parse(ctx);
        ZipParser.Parse(ctx);
        ServicesParser.Parse(ctx);

        return BuildListForTable(ctx);
    }

    /// <summary>
    /// Собирает из заполненного контекста список строк: каждая строка — список значений в порядке TableColumns.Headers.
    /// </summary>
    public static List<List<string>> BuildListForTable(ParserContext ctx)
    {
        var rows = new List<List<string>>();
        var schedule = ctx.WorkSchedule ?? "—";
        var condition = ctx.Condition ?? "—";
        var sla = ctx.Sla ?? "—";
        var slaHigh = ctx.SlaHighLoad ?? "—";
        var zipI = ctx.ZipI ?? "—";
        var zipP = ctx.ZipP ?? "—";
        var zipZ = ctx.ZipZ ?? "—";
        var servicesStr = ctx.Services is { Count: > 0 }
            ? string.Join("; ", ctx.Services)
            : "—";

        var bidTypes = ctx.BidTypes ?? new List<string>();
        var priorities = ctx.Priorities ?? new List<string>();

        if (bidTypes.Count == 0)
        {
            rows.Add(MakeRow(schedule, "—", "—", "—", condition, sla, slaHigh, zipI, zipP, zipZ, servicesStr));
            return rows;
        }

        if (priorities.Count == 0)
        {
            foreach (var bid in bidTypes)
            {
                var qty = GetQuantity(ctx, bid, null);
                rows.Add(MakeRow(schedule, bid, "—", qty, condition, sla, slaHigh, zipI, zipP, zipZ, servicesStr));
            }
            return rows;
        }

        foreach (var bid in bidTypes)
        {
            foreach (var pri in priorities)
            {
                var qty = GetQuantity(ctx, bid, pri);
                rows.Add(MakeRow(schedule, bid, pri, qty, condition, sla, slaHigh, zipI, zipP, zipZ, servicesStr));
            }
        }

        return rows;
    }

    private static string GetQuantity(ParserContext ctx, string bidType, string? priority)
    {
        if (ctx.QuantitiesByKey == null) return "—";
        if (priority != null && ctx.QuantitiesByKey.TryGetValue(priority, out var byPri))
            return byPri;
        if (ctx.QuantitiesByKey.TryGetValue(bidType, out var byBid))
            return byBid;
        return "—";
    }

    /// <summary>Порядок элементов должен строго совпадать с <see cref="TableColumns.Headers"/>.</summary>
    private static List<string> MakeRow(
        string schedule, string bidType, string priority, string quantity,
        string condition, string sla, string slaHigh, string zipI, string zipP, string zipZ,
        string services)
    {
        return new List<string>
        {
            schedule,   // График
            bidType,    // Тип заявки
            priority,   // Приоритет
            quantity,   // Кол-во
            condition,  // Условие
            sla,        // SLA
            slaHigh,    // SLA повышенная нагрузка
            zipI,      // ЗИП-И
            zipP,      // ЗИП-П
            zipZ,      // ЗИП-З
            services   // Услуги
        };
    }
}
