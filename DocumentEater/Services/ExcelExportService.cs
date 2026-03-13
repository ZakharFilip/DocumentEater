using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Заглушка экспорта в Excel. Реализация через Open XML — позже.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    public Task<string> ExportAsync(IReadOnlyList<StructuredRecord> records, string? suggestedFileName = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.Empty);
    }
}
