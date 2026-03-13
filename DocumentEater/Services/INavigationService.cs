using DocumentEater.Models;

namespace DocumentEater.Services;

/// <summary>
/// Навигация между страницами приложения.
/// </summary>
public interface INavigationService
{
    void ShowUploadPage();
    void ShowResultTablePage(IReadOnlyList<StructuredRecord> data);
}
