using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Извлечение текста и таблиц из .docx с помощью Open XML SDK.
/// </summary>
public class WordExtractionService : IWordExtractionService
{
    public async Task<IReadOnlyList<ExtractedTextResult>> ExtractAsync(IReadOnlyList<string> docxPaths, CancellationToken cancellationToken = default)
    {
        if (docxPaths.Count == 0)
            return Array.Empty<ExtractedTextResult>();

        var results = new List<ExtractedTextResult>(docxPaths.Count);
        foreach (var path in docxPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await Task.Run(() => ExtractDocument(path), cancellationToken).ConfigureAwait(false);
            if (result != null)
                results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// Извлекает из документа allText (по параграфам) и allTables. При ошибке логирует и возвращает null.
    /// </summary>
    private static ExtractedTextResult? ExtractDocument(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;
            if (body == null)
                return new ExtractedTextResult
                {
                    DocumentId = Path.GetFileName(filePath),
                    AllText = new List<string>(),
                    AllTables = new List<List<List<string>>>()
                };

            var allText = new List<string>();
            var allTables = new List<List<List<string>>>();

            foreach (var element in body.Elements())
            {
                if (element is Paragraph paragraph)
                {
                    var text = GetParagraphText(paragraph);
                    allText.Add(text);
                }
                else if (element is Table table)
                {
                    var tableData = ExtractTable(table);
                    allTables.Add(tableData);
                }
            }

            return new ExtractedTextResult
            {
                DocumentId = Path.GetFileName(filePath),
                AllText = allText,
                AllTables = allTables
            };
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[WordExtraction] Ошибка обработки документа \"{filePath}\": {ex.Message}");
            return null;
        }
    }

    private static string GetParagraphText(Paragraph paragraph)
    {
        if (paragraph == null) return string.Empty;
        return string.Join("", paragraph.Descendants<Text>().Select(t => t.Text ?? string.Empty));
    }

    private static List<List<string>> ExtractTable(Table table)
    {
        var result = new List<List<string>>();
        if (table == null) return result;

        foreach (var row in table.Elements<TableRow>())
        {
            var rowData = new List<string>();
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = GetCellText(cell);
                var span = GetGridSpan(cell);
                rowData.Add(cellText);
                for (var i = 1; i < span; i++)
                    rowData.Add(string.Empty);
            }
            result.Add(rowData);
        }
        return result;
    }

    private static string GetCellText(TableCell cell)
    {
        if (cell == null) return string.Empty;
        var parts = cell.Descendants<Text>().Select(t => t.Text ?? string.Empty);
        return string.Join(" ", parts);
    }

    private static int GetGridSpan(TableCell cell)
    {
        var tcPr = cell.TableCellProperties;
        if (tcPr == null) return 1;
        var gridSpan = tcPr.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.GridSpan>();
        var val = gridSpan?.Val?.Value;
        return val is > 0 ? val.Value : 1;
    }
}
