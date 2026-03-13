namespace DocumentEater.Models;

/// <summary>
/// Результат извлечения текста и таблиц по одному документу.
/// </summary>
public class ExtractedTextResult
{
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>Все строки текста из документа (по параграфам), порядок сохранён.</summary>
    public List<string> AllText { get; set; } = new();

    /// <summary>Все таблицы документа: [таблица][строка][столбец] → значение ячейки. Пустые ячейки — пустая строка.</summary>
    public List<List<List<string>>> AllTables { get; set; } = new();

    /// <summary>Удобный доступ к AllText как массив (для обратной совместимости).</summary>
    public string[] Lines => AllText.ToArray();
}
