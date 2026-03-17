using System.Collections.Generic;
using DocumentEater.Models;
using DocumentEater.Services.Parsers;

namespace DocumentEater.Services;

/// <summary>
/// Запускает парсинг по каждому документу через GeneralParser и собирает общий список структурированных записей.
/// </summary>
public class SemanticMarkupService : ISemanticMarkupService
{
    public IReadOnlyList<StructuredRecord> Process(IReadOnlyList<ExtractedTextResult> extractedTexts)
    {
        var records = new List<StructuredRecord>();
        var headers = TableColumns.Headers;

        foreach (var doc in extractedTexts ?? [])
        {
            var ctx = new ParserContext
            {
                AllText = doc.AllText ?? new List<string>(),
                AllTables = doc.AllTables ?? new List<List<List<string>>>()
            };
            var rows = GeneralParser.ParseAndBuildTable(ctx);
            foreach (var row in rows)
            {
                // Порядок полей строго по TableColumns.Headers — так столбцы таблицы совпадают с колонками из второго модуля.
                var fields = new Dictionary<string, object?>();
                for (var i = 0; i < headers.Count; i++)
                    fields[headers[i]] = i < row.Count ? row[i] : "—";
                records.Add(new StructuredRecord { Fields = fields });
            }
        }

        return records;
    }
}
