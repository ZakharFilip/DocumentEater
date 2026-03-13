using System.Collections.ObjectModel;
using Avalonia.Input;
using DocumentEater.Models;
using DocumentEater.Services;
using System.Windows.Input;

namespace DocumentEater.ViewModels;

public class UploadPageViewModel : ViewModelBase
{
    private readonly IDocumentStorageService _storage;
    private readonly IWordExtractionService _extraction;
    private readonly ISemanticMarkupService _markup;
    private readonly INavigationService _navigation;

    public UploadPageViewModel(
        IDocumentStorageService storage,
        IWordExtractionService extraction,
        ISemanticMarkupService markup,
        INavigationService navigation)
    {
        _storage = storage;
        _extraction = extraction;
        _markup = markup;
        _navigation = navigation;
        Documents = new ObservableCollection<UploadedDocumentItem>();
        RemoveCommand = new RemoveDocumentCommand(this);
        ProcessCommand = new ProcessDocumentsCommand(this);
        LoadStoredDocuments();
    }

    public ObservableCollection<UploadedDocumentItem> Documents { get; }

    /// <summary>Путь к папке InputDoc (для сохранения через поток, когда локальный путь недоступен).</summary>
    public string InputDirectoryPath => _storage.InputDirectoryPath;

    public ICommand RemoveCommand { get; }
    public ICommand ProcessCommand { get; }

    private bool _isProcessing;
    private string? _statusMessage;
    public bool IsProcessing { get => _isProcessing; private set => SetProperty(ref _isProcessing, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    /// <summary>Добавляет файлы: копирует в InputDoc и обновляет список. Вызывается из View после выбора/перетаскивания.</summary>
    public async Task AddFilesAsync(IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken = default)
    {
        if (sourcePaths.Count == 0) return;
        var saved = await _storage.SaveFilesAsync(sourcePaths, cancellationToken).ConfigureAwait(true);
        foreach (var path in saved)
        {
            var name = Path.GetFileName(path);
            if (Documents.Any(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            Documents.Add(new UploadedDocumentItem(path, name));
        }
    }

    /// <summary>Сохраняет один файл из потока в InputDoc и добавляет в список. Вызывается из View, когда локальный путь недоступен.</summary>
    public async Task AddFileFromStreamAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var savedPath = await _storage.SaveFileFromStreamAsync(sourceStream, fileName, cancellationToken).ConfigureAwait(true);
        if (Documents.Any(d => string.Equals(d.FilePath, savedPath, StringComparison.OrdinalIgnoreCase)))
            return;
        Documents.Add(new UploadedDocumentItem(savedPath, Path.GetFileName(savedPath)));
    }

    public void RemoveDocument(UploadedDocumentItem item)
    {
        _storage.DeleteFile(item.FilePath);
        var idx = Documents.IndexOf(item);
        if (idx >= 0) Documents.RemoveAt(idx);
    }

    private void LoadStoredDocuments()
    {
        Documents.Clear();
        foreach (var path in _storage.GetStoredFilePaths())
            Documents.Add(new UploadedDocumentItem(path, Path.GetFileName(path)));
    }

    private async Task ProcessAsync()
    {
        var selected = Documents.Where(d => d.IsSelected).Select(d => d.FilePath).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Выберите хотя бы один документ.";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Обработка…";
        ((ProcessDocumentsCommand)ProcessCommand).RaiseCanExecuteChanged();
        try
        {
            var extracted = await _extraction.ExtractAsync(selected).ConfigureAwait(true);
            var records = _markup.Process(extracted.ToList());
            _navigation.ShowResultTablePage(records);
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
            ((ProcessDocumentsCommand)ProcessCommand).RaiseCanExecuteChanged();
        }
    }

    public sealed class RemoveDocumentCommand : ICommand
    {
        private readonly UploadPageViewModel _vm;
        public RemoveDocumentCommand(UploadPageViewModel vm) => _vm = vm;
        public bool CanExecute(object? parameter) => parameter is UploadedDocumentItem;
        public void Execute(object? parameter) { if (parameter is UploadedDocumentItem item) _vm.RemoveDocument(item); }
        public event EventHandler? CanExecuteChanged;
    }

    public sealed class ProcessDocumentsCommand : ICommand
    {
        private readonly UploadPageViewModel _vm;
        public ProcessDocumentsCommand(UploadPageViewModel vm) => _vm = vm;
        public bool CanExecute(object? parameter) => !_vm.IsProcessing && _vm.Documents.Any(d => d.IsSelected);
        public async void Execute(object? parameter) => await _vm.ProcessAsync().ConfigureAwait(true);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>Элемент списка документов с поддержкой привязки (CheckBox, удаление).</summary>
public class UploadedDocumentItem : ViewModelBase
{
    private bool _isSelected = true;

    public UploadedDocumentItem(string filePath, string fileName)
    {
        FilePath = filePath;
        FileName = fileName;
    }

    public string FilePath { get; }
    public string FileName { get; }
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
}

/// <summary>Базовый класс для уведомления об изменении свойств.</summary>
public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
