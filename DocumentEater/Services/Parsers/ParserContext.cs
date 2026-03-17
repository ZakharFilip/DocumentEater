namespace DocumentEater.Services.Parsers;

/// <summary>
/// Контекст парсинга одного документа. Передаётся между парсерами, в конце из него собирается listForTable.
/// </summary>
public class ParserContext
{
    public IReadOnlyList<string> AllText { get; set; } = Array.Empty<string>();
    public IReadOnlyList<List<List<string>>> AllTables { get; set; } = Array.Empty<List<List<string>>>();

    /// <summary>Блоки приложений и таблиц: имя блока → (индекс начала строки, индекс конца строки).</summary>
    public Dictionary<string, (int Start, int End)> AppendixBlocks { get; set; } = new();

    public string WorkSchedule { get; set; } = "—";
    public List<string> BidTypes { get; set; } = new();
    public List<string> Priorities { get; set; } = new();
    /// <summary>Кол-во/сроки: ключ — приоритет или тип заявки, значение — текст срока.</summary>
    public Dictionary<string, string> QuantitiesByKey { get; set; } = new();
    public string Condition { get; set; } = "—";
    public string Sla { get; set; } = "—";
    public string SlaHighLoad { get; set; } = "—";
    public string ZipI { get; set; } = "—";
    public string ZipP { get; set; } = "—";
    public string ZipZ { get; set; } = "—";
    public List<string> Services { get; set; } = new();
}

/// <summary>Имена колонок итоговой таблицы.</summary>
public static class TableColumns
{
    public const string График = "График";
    public const string ТипЗаявки = "Тип заявки";
    public const string Приоритет = "Приоритет";
    public const string Колво = "Кол-во";
    public const string Условие = "Условие";
    public const string Sla = "SLA";
    public const string SlaПовышенаяНагрузка = "SLA повышенная нагрузка";
    public const string ЗИПИ = "ЗИП-И";
    public const string ЗИПП = "ЗИП-П";
    public const string ЗИПЗ = "ЗИП-З";
    public const string Услуги = "Услуги";

    public static IReadOnlyList<string> Headers { get; } = new[]
    {
        График, ТипЗаявки, Приоритет, Колво, Условие, Sla, SlaПовышенаяНагрузка, ЗИПИ, ЗИПП, ЗИПЗ, Услуги
    };
}
