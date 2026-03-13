using Avalonia.Controls;
using DocumentEater.Pages;
using DocumentEater.Services;

namespace DocumentEater;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var storage = new DocumentStorageService();
        var extraction = new WordExtractionService();
        var markup = new SemanticMarkupService();
        var navigation = new NavigationService(
            content => MainContent!.Content = content,
            storage,
            extraction,
            markup);
        navigation.ShowUploadPage();
    }
}
