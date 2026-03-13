# Архитектура приложения DocumentEater (Avalonia)

## 1. Общая структура папок и неймспейсов

```
DocumentEater/
├── App.axaml / App.axaml.cs
├── Program.cs
├── MainWindow.axaml / MainWindow.axaml.cs
│
├── Pages/                          # Presentation — страницы (Views)
│   ├── UploadPage.axaml(.cs)       # DocumentEater.Pages
│   └── ResultTablePage.axaml(.cs)
│
├── ViewModels/                     # Presentation — логика представления
│   ├── UploadPageViewModel.cs      # DocumentEater.ViewModels
│   ├── ResultTablePageViewModel.cs
│   └── (общие: BaseViewModel при необходимости)
│
├── Models/                         # Domain / DTO — модели данных
│   ├── UploadedDocument.cs        # DocumentEater.Models
│   ├── ExtractedTextResult.cs
│   ├── StructuredRecord.cs        # одна строка результата разметки
│   └── (при необходимости: ExcelExportResult и т.п.)
│
├── Services/                       # Business Logic + инфраструктура
│   ├── IDocumentStorageService.cs # DocumentEater.Services (интерфейсы)
│   ├── DocumentStorageService.cs
│   ├── IWordExtractionService.cs
│   ├── WordExtractionService.cs
│   ├── ISemanticMarkupService.cs
│   ├── SemanticMarkupService.cs
│   ├── IExcelExportService.cs
│   └── ExcelExportService.cs
│
├── Interfaces/                     # Контракты (опционально вынести сюда из Services)
│   └── (см. ниже — можно хранить интерфейсы в Services/)
│
├── InputDoc/                       # Рабочая папка входящих документов (уже есть)
├── OutputDoc/                      # Рабочая папка исходящих файлов (уже есть)
│
└── (при необходимости)
    └── Helpers/ или Infrastructure/
        └── PathConstants.cs, FileHelper.cs и т.д.
```

**Рекомендация по неймспейсам:**  
Один неймспейс на слой: `DocumentEater.Pages`, `DocumentEater.ViewModels`, `DocumentEater.Models`, `DocumentEater.Services`. Интерфейсы — в `DocumentEater.Services` (или `DocumentEater.Interfaces`, если решите вынести).

---

## 2. Ключевые классы и интерфейсы

### 2.1. Presentation Layer (MVVM)

| Класс / интерфейс | Ответственность |
|-------------------|-----------------|
| **MainWindow** | Контейнер приложения; навигация между страницами (Content = текущая Page), передача зависимостей в дочерние ViewModels при необходимости. |
| **UploadPage** (View) | UI: область Drag-and-Drop, FilePicker, список загруженных документов (CheckBox, кнопка «Удалить»), кнопка «Обработать». Привязки к UploadPageViewModel. |
| **UploadPageViewModel** | Состояние списка документов (ObservableCollection<UploadedDocumentViewModel>), команды: загрузка файлов, удаление, «Обработать». Вызов сервисов хранения и извлечения текста; по завершении — переход на ResultTablePage (через навигацию/событие). |
| **ResultTablePage** (View) | Таблица (DataGrid) для структурированных данных, кнопка «Экспорт в Excel». Привязки к ResultTablePageViewModel. |
| **ResultTablePageViewModel** | Данные для таблицы (коллекция записей), команда экспорта в Excel; вызов SemanticMarkup (если данные приходят «сырыми») и ExcelExportService. |

При необходимости: **INavigationService** / **NavigationService** для смены страниц из ViewModel (инжектируется в ViewModels).

---

### 2.2. Models (Domain / DTO)

| Класс | Ответственность |
|-------|-----------------|
| **UploadedDocument** | Модель документа в списке: путь к файлу, имя файла, флаг «выбран для обработки» (IsSelected). |
| **ExtractedTextResult** | Результат извлечения по одному документу: идентификатор документа (имя/путь), массив строк извлечённого текста. |
| **StructuredRecord** | Одна запись после смысловой разметки — набор полей (словарь или свойства), отображаемых в таблице и экспортируемых в Excel. |
| **UploadedDocumentViewModel** | Опционально: обёртка над UploadedDocument с поддержкой удаления и привязки CheckBox (или те же поля прямо в Model + поведение в UploadPageViewModel). |

Граница: View не знает о сервисах; ViewModel знает только об интерфейсах сервисов и моделях.

---

### 2.3. Services (Business Logic + Data Access)

| Интерфейс / класс | Ответственность |
|-------------------|-----------------|
| **IDocumentStorageService** | Копирование/сохранение загруженных файлов в InputDoc, удаление файла из InputDoc, получение списка файлов в InputDoc. |
| **DocumentStorageService** | Реализация: работа с папкой InputDoc (путь — конфигурируемый или константа). |
| **IWordExtractionService** | Принимает пути к .docx файлам → возвращает коллекцию ExtractedTextResult (по одному на документ). Внутри — Open XML SDK. |
| **WordExtractionService** | Реализация: открытие .docx, извлечение текста в массив строк на документ. |
| **ISemanticMarkupService** | Вход: коллекция массивов строк (или коллекция ExtractedTextResult). Выход: список StructuredRecord. Бизнес-логика разметки (алгоритм — без деталей). |
| **SemanticMarkupService** | Реализация алгоритма смысловой разметки. |
| **IExcelExportService** | Вход: коллекция StructuredRecord, путь сохранения (или только имя файла → сохранение в OutputDoc). Экспорт через Open XML SDK. |
| **ExcelExportService** | Реализация: формирование .xlsx в OutputDoc. |

Слои:  
- **Data Access** по сути — DocumentStorageService (файловая система), WordExtractionService (чтение .docx), ExcelExportService (запись .xlsx).  
- **Business Logic** — SemanticMarkupService и оркестрация в ViewModels (какие документы обрабатывать, порядок вызовов).

---

### 2.4. Навигация и DI (опционально)

| Элемент | Ответственность |
|---------|-----------------|
| **INavigationService** | Методы: ShowUploadPage(), ShowResultTablePage(IEnumerable<StructuredRecord> data) или передача данных через общий state. |
| **NavigationService** | Держит ссылку на MainWindow (или на ContentControl), подменяет Content на нужную Page; создаёт ViewModels для страниц с передачей данных. |
| **Composition root (App / MainWindow)** | Регистрация сервисов (если используется DI-контейнер: Microsoft.Extensions.DependencyInjection или другой) и создание MainWindow с внедрённым INavigationService и прочими зависимостями. |

Без DI: сервисы и навигация создаются в MainWindow/App, в ViewModels передаются через конструктор или фабрику.

---

## 3. Взаимодействие между компонентами

- **MainWindow** владеет текущей страницей и (при необходимости) **INavigationService**. При старте показывает **UploadPage** с **UploadPageViewModel**.
- **UploadPageViewModel** зависит от: **IDocumentStorageService**, **IWordExtractionService**, **INavigationService** (или от события/колбэка для перехода). Не зависит от конкретных View.
- **ResultTablePageViewModel** зависит от: **IExcelExportService**, **ISemanticMarkupService** (если разметка вызывается с этой страницы), и получает данные для таблицы через конструктор или свойство (переданные с UploadPage после обработки).
- **Сервисы** не зависят от UI; они оперируют моделями и путями к файлам.

Зависимости направлены внутрь: Presentation → Services (через интерфейсы), Services → Models. Нет зависимостей от Avalonia в Models и в сервисах (кроме путей к папкам приложения).

---

## 4. Потоки данных между модулями

1. **Загрузка и список документов**  
   Пользователь перетаскивает файлы или выбирает через FilePicker → **UploadPageViewModel** вызывает **IDocumentStorageService** (сохранить в InputDoc, получить список) → обновляет коллекцию документов в View (через binding). Удаление: ViewModel вызывает DocumentStorageService (удалить из папки) и убирает элемент из списка.

2. **Обработка и переход к результатам**  
   Пользователь нажимает «Обработать» → **UploadPageViewModel** собирает отмеченные документы → **IWordExtractionService** извлекает текст (массивы строк по документам) → **ISemanticMarkupService** превращает их в список **StructuredRecord** → ViewModel вызывает **INavigationService.ShowResultTablePage(records)** (или передаёт records в следующую страницу). **MainWindow** подменяет Content на **ResultTablePage**, создаёт **ResultTablePageViewModel** с переданными данными.

3. **Отображение таблицы**  
   **ResultTablePage** биндится к коллекции в **ResultTablePageViewModel** (например, `ObservableCollection<StructuredRecord>` или обёртки для отображения). Данные только для чтения с этой страницы приходят из предыдущего шага.

4. **Экспорт в Excel**  
   Пользователь нажимает «Экспорт в Excel» → **ResultTablePageViewModel** вызывает **IExcelExportService** с текущей коллекцией и (опционально) именем файла → сервис сохраняет .xlsx в **OutputDoc**. При необходимости — диалог выбора пути через отдельный сервис (например, IFileDialogService), чтобы ViewModel не зависел от Avalonia.

Итог: данные идут по цепочке **Upload → Storage → Extraction → SemanticMarkup → ResultTable (View) → ExcelExport → OutputDoc**, с чётким разделением слоёв и ответственности.

---

## 5. Соответствие SOLID

- **S**: Каждый сервис и каждая ViewModel решают одну задачу (хранение, извлечение, разметка, экспорт, отображение списка/таблицы).
- **O**: Новые форматы или источники данных можно добавить новыми реализациями интерфейсов (например, IWordExtractionService → другая реализация), без изменения существующего кода.
- **L**: Замена реализаций интерфейсов (например, заглушки для тестов) не ломает ViewModels.
- **I**: Интерфейсы сервисов узкие (только нужные методы для клиента).
- **D**: ViewModels и оркестраторы зависят от интерфейсов сервисов, а не от конкретных классов; создание реализаций — в composition root (App/MainWindow или DI-контейнер).

Данный документ можно использовать как единственный источник правды при реализации модулей и при добавлении новых страниц или сервисов.
