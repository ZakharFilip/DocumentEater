using System.Collections.ObjectModel;
using DocumentEater.Models;
using DocumentEater.Services.Parsers;

namespace DocumentEater.ViewModels;

/// <summary>
/// ViewModel страницы результатов. Данные и текст для MessageBox передаются при навигации.
/// Колонки вычисляются из данных — таблица подстраивается под любое количество столбцов без изменения кода.
/// </summary>
public class ResultTablePageViewModel
{
    public ResultTablePageViewModel(IReadOnlyList<StructuredRecord> data, string? resultSummary = null)
    {
        var list = data ?? Array.Empty<StructuredRecord>();
        ColumnNames = BuildColumnNames(list);
        Rows = new ObservableCollection<ResultTableRow>(BuildRows(list, ColumnNames));
        ResultSummary = resultSummary ?? string.Empty;
    }

    /// <summary>Имена колонок в порядке отображения: сначала строго по TableColumns.Headers (совпадение со вторым модулем), затем доп. ключи.</summary>
    public IReadOnlyList<string> ColumnNames { get; }

    /// <summary>Строки таблицы для привязки к DataGrid (число строк = число записей из второго модуля).</summary>
    public ObservableCollection<ResultTableRow> Rows { get; }

    /// <summary>Сообщение для показа в диалоге результата работы модуля (например: «Обработано документов: N, строк: M»).</summary>
    public string ResultSummary { get; }

    private static IReadOnlyList<string> BuildColumnNames(IReadOnlyList<StructuredRecord> records)
    {
        var allKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in records)
        {
            if (r.Fields != null)
            {
                foreach (var k in r.Fields.Keys)
                    allKeys.Add(k);
            }
        }

        var ordered = new List<string>();
        foreach (var h in TableColumns.Headers)
        {
            if (allKeys.Remove(h))
                ordered.Add(h);
        }
        foreach (var k in allKeys.OrderBy(x => x, StringComparer.Ordinal))
            ordered.Add(k);

        return ordered;
    }

    private static IEnumerable<ResultTableRow> BuildRows(IReadOnlyList<StructuredRecord> records, IReadOnlyList<string> columnNames)
    {
        foreach (var r in records)
        {
            var values = new object?[columnNames.Count];
            if (r.Fields != null)
            {
                for (var i = 0; i < columnNames.Count; i++)
                    values[i] = r.Fields.TryGetValue(columnNames[i], out var v) ? v : null;
            }
            yield return new ResultTableRow(values);
        }
    }
}
