using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Заглушка: возвращает пустой список. Реализация алгоритма разметки — позже.
/// </summary>
public class SemanticMarkupService : ISemanticMarkupService
{
    public IReadOnlyList<StructuredRecord> Process(IReadOnlyList<ExtractedTextResult> extractedTexts)
    {
        return Array.Empty<StructuredRecord>();
    }
}
