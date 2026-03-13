using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Смысловая разметка: массивы текста → структурированные записи.
/// </summary>
public interface ISemanticMarkupService
{
    IReadOnlyList<StructuredRecord> Process(IReadOnlyList<ExtractedTextResult> extractedTexts);
}
