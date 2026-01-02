
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Models;
using EfcToXamarinAndroid.Core.Repository;
using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.MigrationsHelper.Migrations;

using MauiAppWithMudBlazor.Components.Models;
using Microsoft.Maui.Controls;
using MudBlazor;
using PermissionStatus = EfcToXamarinAndroid.Core.Services.PermissionStatus;

namespace EfcToXamarinAndroid.Core.ViewModels
{
    /// <summary>
    /// Основная ViewModel приложения, управляющая бизнес-логикой и состоянием UI.
    /// </summary>
    public class MainViewModel : IDisposable
    {
        private readonly ISmsReader _smsReader;
        private readonly IFileService _fileService;
        private readonly IDataService _dataService;
        private readonly IUIService _uiService;
        private readonly IPermissionService _permissionService;
        private readonly AppConfiguration _appConfiguration;

        public OperacionTyps CurentType { get; private set; }

        private DateRange? _activeDateRange;
        private decimal? _activeMinAmount;
        private decimal? _activeMaxAmount;
        private string? _activeMcc;

        private string? _activeDescription;
        private string? _activeMccDescription;
        private string? _activeTag;

        public List<FinanceItem> AllItems { get; private set; } = [];
        public Dictionary<OperacionTyps, List<FinanceItem>> FilteredItems { get; private set; } = new();

        #region Статистические свойства
        /// <summary>
        /// Данные первого слайда.
        /// </summary>
        public FlowStatisticsDto FlowStats { get; private set; } = new();
        /// <summary>
        /// Данные второго слайда.
        /// </summary>
        public DynamicsStatisticsDto DynamicsStats { get; private set; } = new();

        public DynamicsStatisticsDto CategoryStats { get; private set; } = new();

        public TabStatisticsDto CurrentStats { get; private set; } = new();

        /// <summary>
        /// Общая сумма доходов за весь период.
        /// </summary>
        public float TotalIncome { get; private set; }
        /// <summary>
        /// Общая сумма расходов за весь период.
        /// </summary>
        public float TotalExpense { get; private set; }
        /// <summary>
        /// Расходы, сгруппированные по категориям (ключ - название категории, значение - сумма).
        /// </summary>
        public Dictionary<string, float> ExpensesByCategory { get; private set; } = new();
        /// <summary>
        /// Общее количество всех операций.
        /// </summary>
        public int TotalTransactionsCount { get; private set; }
        /// <summary>
        /// Количество операций дохода.
        /// </summary>
        public int TotalIncomeCount { get; private set; }
        /// <summary>
        /// Количество операций расхода.
        /// </summary>
        public int TotalExpenseCount { get; private set; }
        /// <summary>
        /// Детальная статистика, сгруппированная по каждому типу операции.
        /// </summary>
        public Dictionary<OperacionTyps, OperationTypeStatistics> StatisticsByOperationType { get; private set; } = new();
        /// <summary>
        /// Количество отфильтрованных операций.
        /// </summary>
        public int FiltredTransactionsCount { get; set; }
        /// <summary>
        /// Сумма отфильтрованных транзакций.
        /// </summary>
        public float FiltredTransactionsSumm { get; set; }


        #endregion

        #region
        /// <summary>
        /// Устанавливает период фильтрации.
        /// </summary>
        public void SetDateRange(DateRange? range)
        {
            _activeDateRange = range;
           
        }

        /// <summary>
        /// Устанавливает минимальную сумму фильтра.
        /// </summary>
        public void SetMinAmount(decimal? min)
        {
            _activeMinAmount = min;
           
        }

        /// <summary>
        /// Устанавливает максимальную сумму фильтра.
        /// </summary>
        public void SetMaxAmount(decimal? max)
        {
            _activeMaxAmount = max;
            
        }

        /// <summary>
        /// Устанавливает MCC код фильтра.
        /// </summary>
        public void SetMcc(string? mcc)
        {
            _activeMcc = mcc;
            
        }

        /// <summary>
        /// Применяет все фильтры одновременно.
        /// </summary>
        public void SetAdvancedFilters(DateRange? range, decimal? minAmount, decimal? maxAmount,
                               string? description, string? mccDescription, string? tag, string? mcc = null)
        {
            _activeDateRange = range;
            _activeMinAmount = minAmount;
            _activeMaxAmount = maxAmount;
            _activeMcc = mcc;

            _activeDescription = description;
            _activeMccDescription = mccDescription;
            _activeTag = tag;

            DataFiltred?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Сбрасывает фильтры.
        /// </summary>
        public void ClearAdvancedFilters()
        {
            _activeDateRange = null;
            _activeMinAmount = null;
            _activeMaxAmount = null;
            _activeMcc = null;

            _activeDescription = null;
            _activeMccDescription = null;
            _activeTag = null;

            DataFiltred?.Invoke(this, EventArgs.Empty);

        }
        #endregion

        public event EventHandler? DataUpdated;
        public event EventHandler? DataFiltred;

        public MainViewModel(
            ISmsReader smsReader,
            IFileService fileService,
            IDataService dataService,
            IUIService uiService,
            IPermissionService permissionService)
        {
            _smsReader = smsReader;
            _fileService = fileService;
            _dataService = dataService;
            _uiService = uiService;
            _permissionService = permissionService;
            var configManager = ConfigurationManager.ConfigManager;
            _appConfiguration = configManager.BankConfigurationFromJson;
        }

        public async Task InitializeAsync()
        {
            _smsReader.SmsReceived += _smsReader_SmsReceived;
            // Инициализация БД и загрузка данных на фоне
            await DatesRepositorio.SetDatasFromDB();  // EF Core
            await LoadFinanceItemsAsync();            // Преобразование DataItem -> FinanceItem
            DatesRepositorio.PaymentsChanged += (_, __) => RefreshData();
            DatesRepositorio.DepositsChanged += (_, __) => RefreshData();
            DatesRepositorio.CashsChanged += (_, __) => RefreshData();
            DatesRepositorio.UnreachableChanged += (_, __) => RefreshData();

        }
        private async Task RefreshData() => await LoadFinanceItemsAsync();
        public async Task LoadFinanceItemsAsync()
        {
            var data = DatesRepositorio.DataItems ?? [];

            AllItems = data
                .Select(item => new FinanceItem
                {
                    Id = item.Id,
                    Date = item.Date,
                    Sum = item.Sum,
                    Description = item.Descripton,
                    Title = item.Title,
                    MccDescription = item.MccDeskription,
                    MCC = item.MCC,
                    UnreachableText = item.UnreachableText,
                    OperationType = item.OperacionTyp,
                    IsNewDataItem = item.IsNewDataItem,
                    Balance = item.Balance
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            UpdateFilteredCache();
            UpdateStatistics();
            DataUpdated?.Invoke(this, EventArgs.Empty);
            await Task.CompletedTask;
        }
        private void UpdateFilteredCache()
        {
            FilteredItems.Clear();
            foreach (OperacionTyps type in Enum.GetValues(typeof(OperacionTyps)))
            {
                FilteredItems[type] = AllItems
                    .Where(f => f.OperationType.ToString() == type.ToString())
                    .ToList();
            }

        }

        /// <summary>
        /// Вычисляет и обновляет статистические показатели на основе текущего списка всех операций.
        /// </summary>
        
        private void UpdateStatistics()
        {
            // Сбрасываем предыдущие значения
            TotalIncome = 0;
            TotalExpense = 0;
            TotalTransactionsCount = 0;
            TotalIncomeCount = 0;
            TotalExpenseCount = 0;
            ExpensesByCategory.Clear();

            // Типы операций, которые считаются расходами
            var expenseTypes = new[] { OperacionTyps.OPLATA, OperacionTyps.NALICHNYE };

            TotalTransactionsCount = AllItems.Count;

            // Рассчитываем общие доходы и расходы
            var incomeItems = AllItems.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).ToList();
            TotalIncome = incomeItems.Sum(i => i.Sum);
            TotalIncomeCount = incomeItems.Count;

            var expenseItems = AllItems.Where(i => expenseTypes.Contains(i.OperationType)).ToList();
            TotalExpense = expenseItems.Sum(i => i.Sum);
            TotalExpenseCount = expenseItems.Count;

            // Группируем расходы по категориям (MccDescription)
            // Используем уже отфильтрованный список расходных операций
            ExpensesByCategory = expenseItems
                .Where(i => !string.IsNullOrEmpty(i.MccDescription))
                .GroupBy(i => i.MccDescription!)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Sum));
        }

        public IEnumerable<FinanceItem> GetFilteredItems(OperacionTyps type)
        {
            CurentType = type;
            IEnumerable<FinanceItem> query = AllItems;

            // 1. Фильтр по табу (тип операции)
            if (type != OperacionTyps.None)
            {
                query = query.Where(x => x.OperationType == type);
            }

            // ... (Существующие фильтры по дате и сумме оставляем как есть) ...
            if (_activeDateRange is not null)
            {
                if (_activeDateRange.Start.HasValue) query = query.Where(x => x.Date.Date >= _activeDateRange.Start.Value.Date);
                if (_activeDateRange.End.HasValue) query = query.Where(x => x.Date.Date <= _activeDateRange.End.Value.Date);
            }
            if (_activeMinAmount.HasValue) query = query.Where(x => (decimal)x.Sum >= _activeMinAmount.Value);
            if (_activeMaxAmount.HasValue) query = query.Where(x => (decimal)x.Sum <= _activeMaxAmount.Value);

            // === НОВЫЕ ФИЛЬТРЫ ===

            // 1. Фильтр по описанию (частичное совпадение)
            if (!string.IsNullOrWhiteSpace(_activeDescription))
            {
                query = query.Where(x => x.Description != null &&
                                         x.Description.Contains(_activeDescription, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Фильтр по категории (MccDescription)
            if (!string.IsNullOrWhiteSpace(_activeMccDescription))
            {
                query = query.Where(x => x.MccDescription != null &&
                                         x.MccDescription.Equals(_activeMccDescription, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Фильтр по Тегу (поиск тега внутри Title)
            if (!string.IsNullOrWhiteSpace(_activeTag))
            {
                // Логика: если тег содержится в Title. 
                // Т.к. вы разбиваете Title по пробелам в GetTags, здесь ищем вхождение слова.
                query = query.Where(x => x.Title != null &&
                                         x.Title.Contains(_activeTag, StringComparison.OrdinalIgnoreCase));
            }

            var result = query.ToList();

            // 5. Обновляем вычисляемые поля
            FiltredTransactionsCount = result.Count;
            FiltredTransactionsSumm = result.Sum(x => x.Sum);
            CalculateAdvancedStats(result);

            CalculateFlowStatistics(result);
            CalculateDynamicsStatistics(result);
            CalculateCategoryStatistics(result);


            return result;
        }
        
        private void CalculateAdvancedStats(IEnumerable<FinanceItem> items)
        {
            var list = items.ToList();
            var stats = new TabStatisticsDto();

            if (list.Count > 0)
            {
                // 1. Среднее, Мин, Макс
                stats.AverageCheck = list.Average(x => x.Sum);
                stats.MaxSum = list.Max(x => x.Sum);
                stats.MinSum = list.Min(x => x.Sum);

                // 2. Аномалии (Топ 3 по сумме)
                stats.Anomalies = list.OrderByDescending(x => x.Sum).Take(3).ToList();

                // 3. Топ 5 MCC и проценты
                // Считаем общую сумму для процентов
                var totalSum = list.Sum(x => x.Sum);
                if (totalSum == 0) totalSum = 1; // защита от деления на 0

                stats.TopCategories = list
                    .Where(x => !string.IsNullOrEmpty(x.MccDescription))
                    .GroupBy(x => x.MccDescription)
                    .Select(g => new CategoryStat
                    {
                        Name = g.Key!,
                        Amount = g.Sum(x => x.Sum),
                        Percentage = (g.Sum(x => x.Sum) / totalSum) * 100
                    })
                    .OrderByDescending(x => x.Amount)
                    .Take(5)
                    .ToList();

                // 4. График (группируем по дням)
                // Берем последние 10 дней, где были операции, чтобы график не был пустым
                var dailyGroups = list
                    .GroupBy(x => x.Date.Date)
                    .OrderBy(g => g.Key)
                    .TakeLast(10) // Ограничиваем точками, чтобы график влез
                    .ToList();

                stats.DailyChartLabels = dailyGroups.Select(g => g.Key.ToString("dd.MM")).ToArray();
                stats.DailyChartData = dailyGroups.Select(g => (double)g.Count()).ToArray();
                // Примечание: Если нужен график сумм, замените g.Count() на g.Sum(x => x.Sum)
            }

            CurrentStats = stats;
        }

        
        private void CalculateFlowStatistics(List<FinanceItem> currentItems)
        {
            var flowStats = new FlowStatisticsDto();

            // Защита от пустых данных
            if (currentItems == null || currentItems.Count == 0)
            {
                flowStats.CurrentPeriodDisplay = DateTime.Now.ToString("MMMM yyyy");
                FlowStats = flowStats;
                return;
            }

            // 1. Расчет Доходов и Расходов
            flowStats.TotalIncome = currentItems.Where(i=>i.OperationType==OperacionTyps.ZACHISLENIE).Sum(x => x.Sum);
            flowStats.TotalExpense = currentItems.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE)
                                                 .Where(i=>i.OperationType!=OperacionTyps.UNREACHABLE)
                                                 .Sum(x => x.Sum);

            // 2. Определение отображаемого периода и границ

            // Берем границы из фильтра, если они установлены
            DateTime startDisplay = _activeDateRange?.Start?.Date ?? currentItems.Min(x => x.Date).Date;
            DateTime endDisplay = _activeDateRange?.End?.Date ?? currentItems.Max(x => x.Date).Date;

            // Если фильтр не установлен, и это не один и тот же день, используем форматирование диапазона
            if (_activeDateRange != null && startDisplay.Date != endDisplay.Date)
            {
                flowStats.CurrentPeriodDisplay = $"{startDisplay:dd MMM} - {endDisplay:dd MMM yyyy}";
            }
            else
            {
                // Если это один день или фильтр не установлен (например, в табе "Все")
                flowStats.CurrentPeriodDisplay = $"{startDisplay:dd MMMM yyyy}";
            }

            // 3. Расчет Дневного Лимита (работает только для текущего или будущего периода)

            var today = DateTime.Now.Date;

            // Определяем, является ли период архивным
            flowStats.IsPeriodInPast = endDisplay < today;

            if (flowStats.IsPeriodInPast)
            {
                flowStats.CalculatedDailyLimit = 0;
            }
            else
            {
                // Для расчета лимита, если период заканчивается в будущем, 
                // берем конец месяца, в который попадает today, или конец фильтра, если он раньше.
                DateTime calculationEndDay = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                // Если конец фильтра раньше конца месяца, используем конец фильтра
                if (endDisplay < calculationEndDay)
                {
                    calculationEndDay = endDisplay;
                }

                // Количество дней, оставшихся для траты (от сегодняшнего дня до конца периода)
                var remainingDays = (calculationEndDay - today).TotalDays + 1;

                if (remainingDays < 1) remainingDays = 1;

                var balance = flowStats.NetFlow;

                // Лимит рассчитываем только для положительного чистого баланса
                flowStats.CalculatedDailyLimit = balance > 0 ? (float)(balance / remainingDays) : 0;
            }

            // Сохраняем результат
            FlowStats = flowStats;
        }
        // Внутри класса MainViewModel
        private void CalculateDynamicsStatistics(List<FinanceItem> currentItems)
        {
            var dynamicsStats = new DynamicsStatisticsDto();

            if (currentItems.Any())
            {
                // 1. Определяем границы для графика
                DateTime minDate = _activeDateRange?.Start?.Date ?? currentItems.Min(x => x.Date).Date;
                DateTime maxDate = _activeDateRange?.End?.Date ?? currentItems.Max(x => x.Date).Date;

                // Если период завершился, берем весь период. Если активен, обрезаем по сегодня.
                DateTime today = DateTime.Now.Date;
                if (maxDate > today) maxDate = today;

            

                // 2. Группируем данные в словари для быстрого поиска
                var incomeByDate = currentItems
                    .Where(i => i.OperationType == OperacionTyps.ZACHISLENIE)
                    .GroupBy(i => i.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Sum));

                var expenseByDate = currentItems
                    .Where(i => i.OperationType != OperacionTyps.ZACHISLENIE)
                    .Where(i => i.OperationType != OperacionTyps.UNREACHABLE)
                    .GroupBy(i => i.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Sum));

                // 3. Цикл по всем дням для создания непрерывного графика (с заполнением нулями)
                var labels = new List<string>();
                var incomeData = new List<double>();
                var expenseData = new List<double>();

                for (DateTime date = minDate; date <= maxDate; date = date.AddDays(1))
                {
                    // Используем только день для подписи, если это не полный год
                    labels.Add(date.ToString("dd.MM"));

                    // Заполняем нулем, если нет данных за этот день
                    incomeData.Add(incomeByDate.ContainsKey(date) ? (double)incomeByDate[date] : 0.0);
                    expenseData.Add(expenseByDate.ContainsKey(date) ? (double)expenseByDate[date] : 0.0);
                }

                dynamicsStats.DailyChartLabels = labels.ToArray();
                dynamicsStats.DailyIncomeData = incomeData.ToArray();
                dynamicsStats.DailyExpenseData = expenseData.ToArray();
            }

            // Присваиваем результат
            DynamicsStats = dynamicsStats;
        }

        private void CalculateCategoryStatistics(List<FinanceItem> currentItems)
        {
            // ... (Расчет FlowStats и DynamicsStats здесь) ...

            // --- Расчет Слайда 3: Категории ---
            var categoryStats = new DynamicsStatisticsDto();

            if (currentItems.Any())
            {
                // Решаем, что считать за "Total" для расчета процентов. 
                // Если выбран таб "Расход", Total = TotalExpense. Если "Доход", Total = TotalIncome.
                // Если "Все" (OperacionTyps.None), то лучше считать расходы, т.к. категории обычно интересны для трат.

                // Определяем базовый список для группировки
                IEnumerable<FinanceItem> itemsToGroup;

                //if (CurentType == OperacionTyps.ZACHISLENIE)
                //{
                //    itemsToGroup = currentItems.Where(i=>i.OperationType== OperacionTyps.ZACHISLENIE);
                //}
                //else // OPLATA, NALICHNYE, or None (показываем расходы по умолчанию)
                //{
                //    itemsToGroup = currentItems.Where(i => i.OperationType == OperacionTyps.OPLATA || i.OperationType == OperacionTyps.NALICHNYE);
                //}

                var totalForCalc = currentItems.Sum(x => x.Sum);
                if (totalForCalc == 0) totalForCalc = 1; // Защита от деления на 0

                categoryStats.TopCategories = currentItems
                    .Where(x => !string.IsNullOrEmpty(x.MccDescription))
                    .GroupBy(x => x.MccDescription)
                    .Select(g => new CategoryStat
                    {
                        Name = g.Key!,
                        Amount = g.Sum(x => x.Sum),
                        Percentage = (g.Sum(x => x.Sum) / totalForCalc) * 100
                    })
                    .OrderByDescending(x => x.Amount)
                    .Take(5)
                    .ToList();
            }

            // Присваиваем результаты
            // FlowStats = flowStats;
            // DynamicsStats = dynamicsStats;
            CategoryStats = categoryStats;
        }

        public IEnumerable<string?> GetDeskriptions(OperacionTyps type)
        {
            if (FilteredItems.TryGetValue(type, out var deskrList))
            {
                var deskrps = deskrList.Select(x => x.Description).Distinct();
                if (deskrps != null)
                    return deskrps;
            }

            return Enumerable.Empty<string>();

        }

        private async void _smsReader_SmsReceived(object? sender, Sms sms)
        {
            try
            {
                var smsList = new List<Sms>() { sms };
                var dataItems = await _dataService.ParseSmsToDataItemsAsync(smsList, _appConfiguration.Banks);
                if (dataItems?.Any() == true)
                {
                    await DatesRepositorio.AddDatas(dataItems);
                }
            }
            catch (Exception ex)
            {
                // Здесь важна логика обработки ошибок, например, логирование
                Console.WriteLine($"Ошибка при обработке полученного SMS: {ex.Message}");
            }
        }

        public async Task ProcessSmsDataAsync()
        {

            var smsList = await _smsReader.GetAllSmsAsync(_appConfiguration.Banks);
            var dataItems = await _dataService.ParseSmsToDataItemsAsync(smsList, _appConfiguration.Banks);

            if (dataItems?.Any() == true)
            {
                await DatesRepositorio.AddDatas(dataItems);
            }
        }

        public async Task ProcessPdfFileAsync(string filePath)
        {
            try
            {
                var configManager = ConfigurationManager.ConfigManager;
                var configuration = configManager.BankConfigurationFromJson;

                var dataItems = await _dataService.ParsePdfToDataItemsAsync(filePath, configuration.Banks);
                await DatesRepositorio.AddDatas(dataItems);

                await _uiService.ShowToastAsync("Файл обработан.");
            }
            catch (Exception ex)
            {
                await _uiService.ShowToastAsync("Произошла ошибка при обработке файла.");
            }
        }

        public async Task ExportDataAsync()
        {
            try
            {
                var downloadsPath = _fileService.GetDownloadsPath(); //await _fileService.PickFolderAsync();//
                var fileName = $"FinReport{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xml";
                var filePath = System.IO.Path.Combine(downloadsPath, fileName);

                var success = await _dataService.SaveDataItemsToXmlAsync(filePath);

                if (success)
                {
                    await _uiService.ShowToastAsync("Данные экспортированы.");
                }
                else
                {
                    await _uiService.ShowToastAsync("Ошибка экспорта данных.");
                }
            }
            catch (Exception ex)
            {
                await _uiService.ShowToastAsync("Ошибка экспорта данных.");
            }
        }

        public async Task ImportDataAsync()
        {
            try
            {
                var filePath = await _fileService.PickFileAsync("Выберите файл для импорта", [".xml"]);
                if (!string.IsNullOrEmpty(filePath))
                {
                    var dataItems = await _dataService.ParseXmlToDataItemsAsync(filePath);
                    await DatesRepositorio.AddDatas(dataItems.ToList());
                    await _uiService.ShowToastAsync("Данные импортированы.");
                }
            }
            catch (Exception ex)
            {
                await _uiService.ShowToastAsync("Ошибка импорта данных.");
            }
        }

        public async Task ClearDatabaseAsync()
        {
            var confirmed = await _uiService.ShowConfirmationDialogAsync(
                "Очистка базы данных",
                "Данное действие приведет к полной очистке базы данных приложения. Вы действительно хотите продолжить?");

            if (confirmed)
            {
                var success = await DatesRepositorio.DeleteAllItems();
                var message = success ? "База данных очищена." : "Произошла ошибка очистки базы данных.";
                await _uiService.ShowToastAsync(message);
            }
        }
        public async Task UpdateItemValueAsync(int id, FinanceItem item)
        {
            DataItem dataItem = new DataItem();// await DatesRepositorio.GetDataItem(id);

            dataItem.Date = item.Date;
            dataItem.Sum = item.Sum;
            dataItem.Descripton = item.Description;
            dataItem.Title = item.Title;
            dataItem.MccDeskription = item.MccDescription;
            dataItem.MCC = item.MCC;
            dataItem.UnreachableText = item.UnreachableText;
            dataItem.OperacionTyp = item.OperationType;
            dataItem.IsNewDataItem = item.IsNewDataItem;
            dataItem.Balance = item.Balance;

            await DatesRepositorio.UpdateItemValue(id, dataItem);
        }
        public async Task<FinanceItem> GetFinItem(int id)
        {
            var item = await DatesRepositorio.GetDataItem(id);
            var finItem = new FinanceItem
            {
                Id = item.Id,
                Date = item.Date,
                Sum = item.Sum,
                Description = item.Descripton,
                Title = item.Title,
                MccDescription = item.MccDeskription,
                MCC = item.MCC,
                UnreachableText = item.UnreachableText,
                OperationType = item.OperacionTyp,
                IsNewDataItem = item.IsNewDataItem,
                Balance = item.Balance
            };
            return finItem;
        }

        public async Task ResetFinItem(FinanceItem item)
        {
            var oldItem = await DatesRepositorio.GetDataItem(item.Id);

            item.Id = oldItem.Id;
            item.Date = oldItem.Date;
            item.Sum = oldItem.Sum;
            item.Description = oldItem.Descripton;
            item.Title = oldItem.Title;
            item.MccDescription = oldItem.MccDeskription;
            item.MCC = oldItem.MCC;
            item.UnreachableText = oldItem.UnreachableText;
            item.OperationType = oldItem.OperacionTyp;
            item.IsNewDataItem = oldItem.IsNewDataItem;
            item.Balance = oldItem.Balance;

        }

        public async Task<bool> CheckPermissionsAsync()
        {
            var smsPermission = await _permissionService.CheckSmsPermissionAsync();
            var storageReadPermission = await _permissionService.CheckStorageReadPermissionAsync();
            var storageWritePermission = await _permissionService.CheckStorageWritePermissionAsync();

            if (smsPermission != PermissionStatus.Granted)
            {
                await _permissionService.RequestSmsPermissionAsync();
            }

            if (storageReadPermission != PermissionStatus.Granted)
            {
                await _permissionService.RequestStorageReadPermissionAsync();
            }

            if (storageWritePermission != PermissionStatus.Granted)
            {
                await _permissionService.RequestStorageWritePermissionAsync();
            }

            return smsPermission == PermissionStatus.Granted &&
                   storageReadPermission == PermissionStatus.Granted &&
                   storageWritePermission == PermissionStatus.Granted;
        }

        public async Task<List<string>> GetTags()
        {
            return DatesRepositorio.GetTags();
        }
        public IEnumerable<int> GetMccCodes(OperacionTyps type)
        {
            if (FilteredItems.TryGetValue(type, out var mccList))
            {
                var mccs = mccList.Select(x => x.MCC).Distinct();
                if (mccs != null)
                    return mccs;
            }

            return Enumerable.Empty<int>();

        }
        public IEnumerable<string> GetMccDeskriptons(OperacionTyps type)
        {
            if (FilteredItems.TryGetValue(type, out var mccDiskrList))
            {
                var mccDs = mccDiskrList.Select(x => x.MccDescription).Distinct();
                if (mccDs != null)
                    return mccDs;
            }

            return Enumerable.Empty<string>();

        }
        // Вставьте следующий метод сюда, например, после метода AddItem()      
        public async Task<IEnumerable<FinanceItem>> GetFinanceItemsChunk(GetItemsRequest request)
        {
            var items = await DatesRepositorio.GetDataItems(request);
            var fintems = items
              .Select(item => new FinanceItem
              {
                  Id = item.Id,
                  Date = item.Date,
                  Sum = item.Sum,
                  Description = item.Descripton,
                  Title = item.Title,
                  MccDescription = item.MccDeskription,
                  MCC = item.MCC,
                  UnreachableText = item.UnreachableText,
                  OperationType = item.OperacionTyp,
                  IsNewDataItem = item.IsNewDataItem,
                  Balance = item.Balance
              })
              .OrderByDescending(x => x.Date)
              .ToList();
            return fintems;
        }

        /// <summary>
        /// Освобождает ресурсы и отписывается от событий, чтобы предотвратить утечки памяти.
        /// </summary>
        public void Dispose()
        {
            _smsReader.SmsReceived -= _smsReader_SmsReceived;
            DatesRepositorio.PaymentsChanged -= (_, __) => RefreshData();
            DatesRepositorio.DepositsChanged -= (_, __) => RefreshData();
            DatesRepositorio.CashsChanged -= (_, __) => RefreshData();
            DatesRepositorio.UnreachableChanged -= (_, __) => RefreshData();
        }
    }
}