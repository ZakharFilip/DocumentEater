using System.Collections.ObjectModel;
using System.Windows.Input;
using DocumentEater.Models;

namespace DocumentEater.ViewModels;

/// <summary>
/// ViewModel страницы конструкторской таблицы.
/// Пока модуль смысловой разметки не реализован, таблица заполняется тестовыми данными.
/// </summary>
public class ResultTablePageViewModel : ViewModelBase
{
    public ResultTablePageViewModel(IReadOnlyList<StructuredRecord> data)
    {
        Rows = new ObservableCollection<ResultRowViewModel>();
        ExportToExcelCommand = new ExportToExcelCommandImpl();

        // Модуль 2 ещё не реализован – игнорируем входные данные и заполняем тестовыми строками.
        LoadTestData();
    }

    /// <summary>Строки таблицы, которые отображаются во View.</summary>
    public ObservableCollection<ResultRowViewModel> Rows { get; }

    /// <summary>Команда выгрузки в Excel (реализация будет в модуле 4).</summary>
    public ICommand ExportToExcelCommand { get; }

    private void LoadTestData()
    {
        // TODO: заменить на реальные данные из модуля 2, когда он будет готов.
        // Структура и значения основаны на шаблонной таблице ExapleOfTable.xlsx.

        Rows.Clear();

        // Пример: пять строк с условными данными (все значения как строки).
        Rows.Add(new ResultRowViewModel("1", "Документ 1", "Тип A", "01.01.2024", "Комментарий 1"));
        Rows.Add(new ResultRowViewModel("2", "Документ 2", "Тип B", "05.01.2024", "Комментарий 2"));
        Rows.Add(new ResultRowViewModel("3", "Документ 3", "Тип A", "10.01.2024", "Комментарий 3"));
        Rows.Add(new ResultRowViewModel("4", "Документ 4", "Тип C", "15.01.2024", "Комментарий 4"));
        Rows.Add(new ResultRowViewModel("5", "Документ 5", "Тип B", "20.01.2024", "Комментарий 5"));
    }

    /// <summary>Одна строка результирующей таблицы.</summary>
    public class ResultRowViewModel : ViewModelBase
    {
        private string _col1;
        private string _col2;
        private string _col3;
        private string _col4;
        private string _col5;

        public ResultRowViewModel(string col1, string col2, string col3, string col4, string col5)
        {
            _col1 = col1;
            _col2 = col2;
            _col3 = col3;
            _col4 = col4;
            _col5 = col5;
        }

        public string Col1 { get => _col1; set => SetProperty(ref _col1, value); }
        public string Col2 { get => _col2; set => SetProperty(ref _col2, value); }
        public string Col3 { get => _col3; set => SetProperty(ref _col3, value); }
        public string Col4 { get => _col4; set => SetProperty(ref _col4, value); }
        public string Col5 { get => _col5; set => SetProperty(ref _col5, value); }
    }

    /// <summary>Пока команда-заглушка для будущего экспорта в Excel.</summary>
    private sealed class ExportToExcelCommandImpl : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { /* реализация появится в модуле 4 */ }
        public event EventHandler? CanExecuteChanged;
    }
}
