using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using DocumentEater.ViewModels;

namespace DocumentEater.Pages;

public partial class UploadPage : UserControl
{
    public UploadPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PickFilesButton.Click += OnPickFilesClick;
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop, Avalonia.Interactivity.RoutingStrategies.Bubble);
        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver, Avalonia.Interactivity.RoutingStrategies.Bubble);
    }

    private async void OnPickFilesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || DataContext is not UploadPageViewModel vm)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = "Выберите документы Word",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Документы Word") { Patterns = new[] { "*.docx" } }
            }
        }).ConfigureAwait(true);

        if (files.Count == 0)
            return;

        var pathsToCopy = new List<string>();
        foreach (var file in files)
        {
            if (!file.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                continue;
            var path = GetLocalPath(file);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                pathsToCopy.Add(path);
            else
            {
                await using var stream = await file.OpenReadAsync().ConfigureAwait(true);
                await vm.AddFileFromStreamAsync(stream, file.Name).ConfigureAwait(true);
            }
        }
        if (pathsToCopy.Count > 0)
            await vm.AddFilesAsync(pathsToCopy).ConfigureAwait(true);
    }

    private static string? GetLocalPath(IStorageItem item)
    {
        var path = item.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
            return path;
        if (item.Path != null && item.Path.IsAbsoluteUri && string.Equals(item.Path.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            return item.Path.LocalPath;
        return null;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
#pragma warning restore CS0618
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618
        if (DataContext is not UploadPageViewModel vm || !e.Data.Contains(DataFormats.Files))
            return;
        var items = e.Data.GetFiles()?.ToList() ?? new List<IStorageItem>();
#pragma warning restore CS0618
        var pathsToCopy = new List<string>();
        foreach (var item in items)
        {
            var name = item.Name ?? string.Empty;
            if (!name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                continue;
            var path = GetLocalPath(item);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                pathsToCopy.Add(path);
            else if (item is IStorageFile file)
            {
                await using var stream = await file.OpenReadAsync().ConfigureAwait(true);
                await vm.AddFileFromStreamAsync(stream, name).ConfigureAwait(true);
            }
        }
        if (pathsToCopy.Count > 0)
            await vm.AddFilesAsync(pathsToCopy).ConfigureAwait(true);
    }
}
