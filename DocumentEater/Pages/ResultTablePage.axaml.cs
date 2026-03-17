using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using DocumentEater.Models;
using DocumentEater.ViewModels;

namespace DocumentEater.Pages;

public partial class ResultTablePage : UserControl
{
    public ResultTablePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ResultTablePageViewModel vm)
            return;

        BuildDataGridColumns(vm);

        if (string.IsNullOrWhiteSpace(vm.ResultSummary))
            return;

        var owner = TopLevel.GetTopLevel(this) as Window;
        var dialog = new Window
        {
            Title = "Результат работы модуля",
            Width = 420,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = BuildDialogContent(vm.ResultSummary, out var okButton)
        };
        okButton.Click += (_, _) => dialog.Close();

        if (owner != null)
            await dialog.ShowDialog(owner);
        else
            dialog.Show();
    }

    private static StackPanel BuildDialogContent(string message, out Button okButton)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 16
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 360
        });
        okButton = new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        panel.Children.Add(okButton);
        return panel;
    }

    private void BuildDataGridColumns(ResultTablePageViewModel vm)
    {
        var grid = this.FindControl<DataGrid>("ResultGrid");
        if (grid == null) return;

        grid.Columns.Clear();
        for (var i = 0; i < vm.ColumnNames.Count; i++)
        {
            var header = vm.ColumnNames[i];
            var index = i;
            var col = new DataGridTemplateColumn
            {
                Header = header,
                Width = new DataGridLength(160, DataGridLengthUnitType.Pixel),
                MinWidth = 100,
                CellTemplate = new FuncDataTemplate<ResultTableRow>((_, _) =>
                {
                    var textBlock = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(6, 4)
                    };
                    textBlock.Bind(TextBlock.TextProperty, new Binding($"Values[{index}]") { Converter = new CellValueConverter() });
                    return textBlock;
                })
            };
            grid.Columns.Add(col);
        }

        // Разрешаем рост строк по содержимому (если грид поддерживает NaN как авто-высоту)
        try { grid.RowHeight = double.NaN; } catch { /* фиксированная высота по умолчанию */ }
    }
}

internal sealed class CellValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value?.ToString() ?? string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value;
}
