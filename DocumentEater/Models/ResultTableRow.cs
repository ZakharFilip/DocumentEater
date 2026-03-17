namespace DocumentEater.Models;

/// <summary>
/// Строка таблицы для отображения: значения в фиксированном порядке колонок (по индексу).
/// Используется для универсальной таблицы с динамическим числом столбцов.
/// </summary>
public class ResultTableRow
{
    /// <summary>Значения ячеек в порядке колонок (индекс = номер столбца).</summary>
    public IReadOnlyList<object?> Values { get; }

    public ResultTableRow(IReadOnlyList<object?> values)
    {
        Values = values ?? Array.Empty<object?>();
    }
}
