namespace DocumentEater.Services;

/// <summary>
/// Реализация работы с папкой InputDoc (файловая система).
/// </summary>
public class DocumentStorageService : IDocumentStorageService
{
    private readonly string _inputDirectoryPath;

    public DocumentStorageService(string? inputDirectoryPath = null)
    {
        _inputDirectoryPath = inputDirectoryPath ?? Path.Combine(AppContext.BaseDirectory, "InputDoc");
        EnsureInputDirectoryExists();
    }

    public string InputDirectoryPath => _inputDirectoryPath;

    public async Task<IReadOnlyList<string>> SaveFilesAsync(IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken = default)
    {
        if (sourcePaths.Count == 0)
            return Array.Empty<string>();

        EnsureInputDirectoryExists();
        var savedPaths = new List<string>(sourcePaths.Count);

        foreach (var sourcePath in sourcePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                continue;

            var fileName = Path.GetFileName(sourcePath);
            var destPath = Path.Combine(_inputDirectoryPath, fileName);

            // избегаем перезаписи: добавляем номер, если файл уже есть
            destPath = GetUniqueFilePath(destPath);
            await Task.Run(() => File.Copy(sourcePath, destPath), cancellationToken).ConfigureAwait(false);
            savedPaths.Add(destPath);
        }

        return savedPaths;
    }

    public async Task<string> SaveFileFromStreamAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName is required.", nameof(fileName));
        EnsureInputDirectoryExists();
        var destPath = Path.Combine(_inputDirectoryPath, Path.GetFileName(fileName));
        destPath = GetUniqueFilePath(destPath);
        await using (var destStream = File.Create(destPath))
            await sourceStream.CopyToAsync(destStream, cancellationToken).ConfigureAwait(false);
        return destPath;
    }

    public void DeleteFile(string storedFilePath)
    {
        if (string.IsNullOrWhiteSpace(storedFilePath))
            return;
        if (!Path.GetFullPath(storedFilePath).StartsWith(Path.GetFullPath(_inputDirectoryPath), StringComparison.OrdinalIgnoreCase))
            return;
        if (File.Exists(storedFilePath))
            File.Delete(storedFilePath);
    }

    public IReadOnlyList<string> GetStoredFilePaths()
    {
        EnsureInputDirectoryExists();
        return Directory
            .GetFiles(_inputDirectoryPath, "*.docx", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void EnsureInputDirectoryExists()
    {
        if (!Directory.Exists(_inputDirectoryPath))
            Directory.CreateDirectory(_inputDirectoryPath);
    }

    private static string GetUniqueFilePath(string path)
    {
        if (!File.Exists(path))
            return path;
        var dir = Path.GetDirectoryName(path) ?? path;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        for (var i = 1; i < 10000; i++)
        {
            var candidate = Path.Combine(dir, $"{name}_{i}{ext}");
            if (!File.Exists(candidate))
                return candidate;
        }
        return Path.Combine(dir, $"{name}_{Guid.NewGuid():N}{ext}");
    }
}
