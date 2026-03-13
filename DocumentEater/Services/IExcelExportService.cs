using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Экспорт структурированных данных в Excel (OutputDoc).
/// </summary>
public interface IExcelExportService
{
    Task<string> ExportAsync(IReadOnlyList<StructuredRecord> records, string? suggestedFileName = null, CancellationToken cancellationToken = default);
}
