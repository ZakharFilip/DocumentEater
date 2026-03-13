namespace DocumentEater.Models;

/// <summary>
/// Модель документа в списке загрузки: путь к файлу, имя, флаг участия в обработке.
/// </summary>
public class UploadedDocument
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = true;
}
