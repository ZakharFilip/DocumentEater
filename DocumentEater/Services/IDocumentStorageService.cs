namespace DocumentEater.Services;

/// <summary>
/// Сохранение загруженных файлов в InputDoc, удаление, получение списка.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>Копирует файлы в папку InputDoc и возвращает пути сохранённых файлов.</summary>
    Task<IReadOnlyList<string>> SaveFilesAsync(IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken = default);

    /// <summary>Сохраняет содержимое потока в InputDoc под указанным именем. Возвращает путь к сохранённому файлу.</summary>
    Task<string> SaveFileFromStreamAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Удаляет файл из InputDoc по пути.</summary>
    void DeleteFile(string storedFilePath);

    /// <summary>Возвращает пути всех файлов в InputDoc.</summary>
    IReadOnlyList<string> GetStoredFilePaths();

    /// <summary>Путь к папке InputDoc.</summary>
    string InputDirectoryPath { get; }
}
