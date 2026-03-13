using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Извлечение текста из .docx через Open XML SDK.
/// </summary>
public interface IWordExtractionService
{
    /// <summary>Извлекает текст из указанных .docx файлов. Один результат на документ.</summary>
    Task<IReadOnlyList<ExtractedTextResult>> ExtractAsync(IReadOnlyList<string> docxPaths, CancellationToken cancellationToken = default);
}
