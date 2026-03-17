namespace DocumentEater.Services.Parsers;

/// <summary>
/// Шаг 7: Извлекает Условие из текста и таблиц (AllTables): По требованию, По запросу, по согласованию и т.д.
/// </summary>
public static class ConditionParser
{
    private static readonly string[] ConditionPhrases = new[]
    {
        "По требованию заказчика",
        "По требованию пользователя",
        "По требованию",
        "По треб.",
        "По запросу Заказчика",
        "По запросу Пользователя",
        "По запросу заказчика",
        "По запросу пользователя",
        "По запросу",
        "по согласованию",
        "По согласованию",
        "по согласованию сторон"
    };

    public static void Parse(ParserContext ctx)
    {
        ctx.Condition = "—";
        var fullText = string.Join(" ", ctx.AllText);

        foreach (var phrase in ConditionPhrases)
        {
            if (fullText.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                ctx.Condition = phrase.Trim();
                return;
            }
        }

        // Поиск в таблицах AllTables
        foreach (var table in ctx.AllTables)
        {
            foreach (var row in table)
            {
                var rowText = string.Join(" ", row);
                foreach (var phrase in ConditionPhrases)
                {
                    if (rowText.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Condition = phrase.Trim();
                        return;
                    }
                }
            }
        }
    }
}
