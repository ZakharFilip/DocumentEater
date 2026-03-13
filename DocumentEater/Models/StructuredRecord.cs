namespace DocumentEater.Models;

/// <summary>
/// Одна запись после смысловой разметки — набор полей для таблицы и экспорта в Excel.
/// </summary>
public class StructuredRecord
{
    public Dictionary<string, object?> Fields { get; set; } = new();
}
