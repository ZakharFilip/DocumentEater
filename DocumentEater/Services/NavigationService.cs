using DocumentEater.Models;
using DocumentEater.Pages;
using DocumentEater.ViewModels;

namespace DocumentEater.Services;

/// <summary>
/// Реализация навигации: подмена контента главного окна.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Action<object> _setMainContent;
    private readonly IDocumentStorageService _storage;
    private readonly IWordExtractionService _extraction;
    private readonly ISemanticMarkupService _markup;

    public NavigationService(
        Action<object> setMainContent,
        IDocumentStorageService storage,
        IWordExtractionService extraction,
        ISemanticMarkupService markup)
    {
        _setMainContent = setMainContent;
        _storage = storage;
        _extraction = extraction;
        _markup = markup;
    }

    public void ShowUploadPage()
    {
        var vm = new UploadPageViewModel(_storage, _extraction, _markup, this);
        _setMainContent(new UploadPage { DataContext = vm });
    }

    public void ShowResultTablePage(IReadOnlyList<StructuredRecord> data)
    {
        var vm = new ResultTablePageViewModel(data);
        _setMainContent(new ResultTablePage { DataContext = vm });
    }
}
