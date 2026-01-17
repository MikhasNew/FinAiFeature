
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Models;
using EfcToXamarinAndroid.Core.Repository;
using EfcToXamarinAndroid.Core.Services;
using EfcToXamarinAndroid.MigrationsHelper.Migrations;
using Microsoft.AspNetCore.Components.Web.Virtualization;

using EfcToXamarinAndroid.UI.Components.Models;



using MudBlazor;
using PermissionStatus = EfcToXamarinAndroid.Core.Services.PermissionStatus;
using EfcToXamarinAndroid.Core.Enums;

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
        private readonly IQrCodeService _qrCodeService;
        private readonly AppConfiguration _appConfiguration;

        public OperacionTyps CurentType { get; private set; }

        private DateRange? _activeDateRange;
        private decimal? _activeMinAmount;
        private decimal? _activeMaxAmount;
        private string? _activeMcc;

        private string? _activeDescription;
        private string? _activeMccDescription;
        private string? _activeTag;


        public Dictionary<int, string> MccCodes =>
            MccConfigurationManager.ConfigManager.MccConfigurationFromJson;
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
        public string DynamicsGroupingTitle { get; private set; } = "ДИНАМИКА";
        public double ChartYAxisMax { get; private set; } = 1000;

        public DynamicsStatisticsDto CategoryStats { get; private set; } = new();
        public PredictionStatisticsDto PredictionStats { get; private set; } = new();

        public TabStatisticsDto CurrentStats { get; private set; } = new();
        
        #region Pre-calculated view values
        public double AvgIncome { get; private set; }
        public double AvgExpense { get; private set; }
        public double MaxIncome { get; private set; }
        public double MaxExpense { get; private set; }
        #endregion

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

        public bool IsAdvancedFilterActive { get; private set; }
        
        public bool IsFiltred => IsAdvancedFilterActive;

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
            IsAdvancedFilterActive = true;

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
            
            IsAdvancedFilterActive = false;
            ApplyDefaultPeriod();

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
            IPermissionService permissionService,
            IQrCodeService qrCodeService)
        {
            _smsReader = smsReader;
            _fileService = fileService;
            _dataService = dataService;
            _uiService = uiService;
            _permissionService = permissionService;
            _qrCodeService = qrCodeService;
            var configManager = EfcToXamarinAndroid.Core.Configs.ManagerCore.ConfigurationManager.ConfigManager;
            _appConfiguration = configManager.BankConfigurationFromJson;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _smsReader.SmsReceived += _smsReader_SmsReceived;
                
                // Запрашиваем разрешения уровня платформы
                await CheckPermissionsAsync();

                // Инициализация БД и загрузка данных
                await DatesRepositorio.SetDatasFromDB();  // EF Core

                // Первичное сканирование SMS (если есть конфигурация)
                if (_appConfiguration?.Banks != null)
                {
                    await ProcessSmsDataAsync();
                    // Запускаем прослушивание новых SMS
                    _smsReader.StartListening(_appConfiguration.Banks);
                }

                await LoadFinanceItemsAsync();            // Преобразование DataItem -> FinanceItem
                
                // Применяем период отображения по умолчанию
                ApplyDefaultPeriod();

                // Подписываемся на статические события через именованный метод, чтобы можно было отписаться
                DatesRepositorio.PaymentsChanged += OnDataChanged;
                DatesRepositorio.DepositsChanged += OnDataChanged;
                DatesRepositorio.CashsChanged += OnDataChanged;
                DatesRepositorio.UnreachableChanged += OnDataChanged;
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не даем приложению упасть намертво
                System.Diagnostics.Debug.WriteLine($"InitializeAsync Error: {ex}");
                await _uiService.ShowToastAsync($"Ошибка при запуске: {ex.Message}");
            }
        }

        // Обработчик изменений в репозитории
        private async void OnDataChanged(object? sender, EventArgs e)
        {
            await RefreshData();
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
                    Balance = item.Balance,
                    PendingQrCode = item.PendingQrCode
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

            // Прогноз на основе всех данных (для initial load)
            CalculatePredictionStatistics(AllItems.ToList());
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
            else
                query = query.Where(x => x.OperationType != OperacionTyps.UNREACHABLE);

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

            // Обновляем базовые счетчики
            FiltredTransactionsCount = result.Count;
            FiltredTransactionsSumm = result.Sum(x => x.Sum);

            _currentFilteredList = result;
            return result;
        }

        private bool _isCalculating = false;
        public bool IsCalculating => _isCalculating;

        /// <summary>
        /// Пересчитывает все статистики на основе текущего отфильтрованного списка.
        /// Вызывается с debouncing из UI для оптимизации производительности.
        /// </summary>
        public async Task RecalculateStatisticsAsync()
        {
            if (_currentFilteredList == null || _currentFilteredList.Count == 0)
            {
                ClearStats();
                return;
            }

            _isCalculating = true;
            try
            {
                var capturedList = _currentFilteredList.ToList();
                await Task.Run(() =>
                {
                    CalculateAdvancedStats(capturedList);
                    CalculateFlowStatistics(capturedList);
                    CalculateDynamicsStatistics(capturedList);
                    CalculateCategoryStatistics(capturedList);
                    CalculatePredictionStatistics(capturedList);
                    
                    // Pre-calculate view values
                    AvgIncome = DynamicsStats.DailyIncomeData?.DefaultIfEmpty(0).Average() ?? 0;
                    AvgExpense = DynamicsStats.DailyExpenseData?.DefaultIfEmpty(0).Average() ?? 0;
                    MaxIncome = DynamicsStats.DailyIncomeData?.DefaultIfEmpty(0).Max() ?? 0;
                    MaxExpense = DynamicsStats.DailyExpenseData?.DefaultIfEmpty(0).Max() ?? 0;
                });
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void ClearStats()
        {
            AvgIncome = 0;
            AvgExpense = 0;
            MaxIncome = 0;
            MaxExpense = 0;
            // Additional clearing if needed
        }
        
        [Obsolete("Use RecalculateStatisticsAsync")]
        public void RecalculateStatistics()
        {
             _ = RecalculateStatisticsAsync();
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

                // 2. Аномалии (Выбросы)
                // Используем порог: более чем в 2.5 раза выше среднего чека
                double threshold = stats.AverageCheck * 2.5;
                stats.Anomalies = list
                    .Where(x => x.Sum > threshold)
                    .OrderByDescending(x => x.Sum)
                    .Take(5)
                    .ToList();

                // Если явных выбросов нет, можно оставить список пустым или взять топ-1 самый дорогой
                // но лучше честно показывать, что выбросов нет. 

                // 3. Топ 5 MCC и проценты
                var totalSum = list.Sum(x => x.Sum);
                if (totalSum == 0) totalSum = 1;

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

                // 4. График
                var dailyGroups = list
                    .GroupBy(x => x.Date.Date)
                    .OrderBy(g => g.Key)
                    .TakeLast(10)
                    .ToList();

                stats.DailyChartLabels = dailyGroups.Select(g => g.Key.ToString("dd.MM")).ToArray();
                stats.DailyChartData = dailyGroups.Select(g => (double)g.Count()).ToArray();

                // 5. РАСЧЕТ СРАВНЕНИЯ (Variant 1)
                DateTime start = _activeDateRange?.Start?.Date ?? (AllItems.Any() ? AllItems.Min(x => x.Date).Date : DateTime.Today.AddDays(-30));
                DateTime end = _activeDateRange?.End?.Date ?? (AllItems.Any() ? AllItems.Max(x => x.Date).Date : DateTime.Today);
                
                var duration = end - start;
                if (duration.TotalDays < 1) duration = TimeSpan.FromDays(30);
                
                DateTime prevStart = start.Add(-duration);
                DateTime prevEnd = start;

                  // Создаем запрос к полному списку, применяя все активные фильтры (кроме даты)
                  var baseQuery = AllItems.AsEnumerable();

                  if (!string.IsNullOrWhiteSpace(_activeDescription))
                      baseQuery = baseQuery.Where(x => x.Description != null && x.Description.Contains(_activeDescription, StringComparison.OrdinalIgnoreCase));
                  
                  if (!string.IsNullOrWhiteSpace(_activeMccDescription))
                      baseQuery = baseQuery.Where(x => x.MccDescription != null && x.MccDescription.Equals(_activeMccDescription, StringComparison.OrdinalIgnoreCase));
                  
                  if (!string.IsNullOrWhiteSpace(_activeTag))
                       baseQuery = baseQuery.Where(x => x.Title != null && x.Title.Contains(_activeTag, StringComparison.OrdinalIgnoreCase));

                  if (CurentType != OperacionTyps.None)
                       baseQuery = baseQuery.Where(x => x.OperationType == CurentType);

                  var prevItems = baseQuery.Where(x => x.Date >= prevStart && x.Date < prevEnd).ToList();
                
                stats.TotalIncome = (float)list.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).Sum(i => i.Sum);
                stats.TotalExpense = (float)list.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(i => i.Sum);
                
                stats.LastMonthTotalIncome = (float)prevItems.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).Sum(i => i.Sum);
                stats.LastMonthTotalExpense = (float)prevItems.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(i => i.Sum);
            }

            CurrentStats = stats;
        }

        private void CalculatePredictionStatistics(List<FinanceItem> currentItems)
        {
            var stats = new PredictionStatisticsDto();

            // Если нет данных для анализа, возвращаем пустую статистику
            if (currentItems == null || currentItems.Count == 0)
            {
                PredictionStats = stats;
                return;
            }

            // 1. Get Current Balance (Latest non-zero balance)
            var latestItemWithBalance = currentItems
                .OrderByDescending(x => x.Date)
                .FirstOrDefault(x => x.Balance > 0);
            
            // Если баланс не найден в транзакциях, пробуем рассчитать как (Доход - Расход) за текущий период
            if (latestItemWithBalance != null)
            {
                stats.CurrentBalance = latestItemWithBalance.Balance;
            }
            else
            {
                // Fallback estimate based on filtered data
                var totalInc = currentItems.Where(x => x.OperationType == OperacionTyps.ZACHISLENIE).Sum(x => x.Sum);
                var totalExp = currentItems.Where(x => x.OperationType != OperacionTyps.ZACHISLENIE && x.OperationType != OperacionTyps.UNREACHABLE).Sum(x => x.Sum);
                stats.CurrentBalance = totalInc - totalExp;
            }
            
            // 2. Predict Next Income
            // Strategy: Look at significant income in the last 90 days (from filtered data)
            var threeMonthsAgo = DateTime.Today.AddDays(-90);
            var incomeItems = currentItems
                .Where(x => x.OperationType == OperacionTyps.ZACHISLENIE && x.Date >= threeMonthsAgo)
                .ToList();

            if (incomeItems.Any())
            {
                // Filter "significant" incomes (e.g., > 20% of max income) 
                var maxInc = incomeItems.Max(x => x.Sum);
                var significantIncomes = incomeItems.Where(x => x.Sum > maxInc * 0.2).ToList(); 

                if (significantIncomes.Any())
                {
                    // Group by Day of Month
                    var frequentDays = significantIncomes
                        .GroupBy(x => x.Date.Day)
                        .Select(g => new { Day = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(2) 
                        .Select(x => x.Day)
                        .OrderBy(x => x)
                        .ToList();

                    if (frequentDays.Any())
                    {
                        var today = DateTime.Today;
                        DateTime? nextDate = null;

                        // Try to find a date in the current month matching one of the days
                        foreach (var day in frequentDays)
                        {
                            try {
                                var candidate = new DateTime(today.Year, today.Month, day);
                                if (candidate > today)
                                {
                                    nextDate = candidate;
                                    break; 
                                }
                            } catch {} 
                        }

                        // If not found in current month, look in next month
                        if (nextDate == null)
                        {
                             foreach (var day in frequentDays)
                             {
                                try {
                                    var candidate = new DateTime(today.Year, today.Month, 1).AddMonths(1);
                                    var daysInNextMonth = DateTime.DaysInMonth(candidate.Year, candidate.Month);
                                    var safeDay = Math.Min(day, daysInNextMonth);
                                    candidate = new DateTime(candidate.Year, candidate.Month, safeDay);
                                    
                                    nextDate = candidate;
                                    break; 
                                } catch {}
                             }
                        }
                        
                        stats.PredictedIncomeDate = nextDate;
                    }
                }
            }

            // Fallback: If no prediction possible, assume End of Month
            if (stats.PredictedIncomeDate == null)
            {
                 var today = DateTime.Today;
                 stats.PredictedIncomeDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            // 3. Calculate Daily Budget
            var daysUntil = (stats.PredictedIncomeDate.Value - DateTime.Today).TotalDays;
            if (daysUntil < 1) daysUntil = 1; // Avoid division by zero
            
            stats.DaysUntilIncome = (int)daysUntil;

            // Only calculate budget if balance is positive
            if (stats.CurrentBalance > 0)
            {
                // Simple strategy: Evenly distribute current balance
                stats.RecommendedDailyBudget = stats.CurrentBalance / daysUntil;
            }
            else
            {
                stats.RecommendedDailyBudget = 0;
            }

            PredictionStats = stats;
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

        private void CalculateDynamicsStatistics(List<FinanceItem> currentItems)
        {
            var dynamicsStats = new DynamicsStatisticsDto();

            if (currentItems.Any())
            {
                // 1. Определяем границы для графика
                DateTime minDate = _activeDateRange?.Start?.Date ?? currentItems.Min(x => x.Date).Date;
                DateTime maxDate = _activeDateRange?.End?.Date ?? currentItems.Max(x => x.Date).Date;

                DateTime today = DateTime.Now.Date;
                if (maxDate > today) maxDate = today;

                var totalDays = (maxDate - minDate).TotalDays;
                
                var labels = new List<string>();
                var incomeData = new List<double>();
                var expenseData = new List<double>();

                if (totalDays > 730) // Более 2 лет -> Группировка по годам
                {
                    DynamicsGroupingTitle = "ПО ГОДАМ";
                    var current = new DateTime(minDate.Year, 1, 1);
                    var end = new DateTime(maxDate.Year, 1, 1);

                    while (current <= end)
                    {
                        labels.Add(current.ToString("yyyy"));
                        
                        // Берем данные за весь год
                        var yearEnd = current.AddYears(1).AddSeconds(-1);
                        var chunk = currentItems.Where(x => x.Date >= current && x.Date <= yearEnd).ToList();

                        incomeData.Add(chunk.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).Sum(i => i.Sum));
                        expenseData.Add(chunk.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(i => i.Sum));

                        current = current.AddYears(1);
                    }
                }
                else if (totalDays > 60) // Более 2 месяцев -> Группировка по месяцам
                {
                    DynamicsGroupingTitle = "ПО МЕСЯЦАМ";
                    var current = new DateTime(minDate.Year, minDate.Month, 1);
                    var end = new DateTime(maxDate.Year, maxDate.Month, 1);

                    while (current <= end)
                    {
                        labels.Add(current.ToString("MMM yy"));
                        
                        // Берем данные за весь месяц
                        var monthEnd = current.AddMonths(1).AddSeconds(-1);
                        var chunk = currentItems.Where(x => x.Date >= current && x.Date <= monthEnd).ToList();

                        incomeData.Add(chunk.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).Sum(i => i.Sum));
                        expenseData.Add(chunk.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(i => i.Sum));

                        current = current.AddMonths(1);
                    }
                }
                else if (totalDays > 21) // От 3 недель до 2 месяцев -> Группировка по неделям
                {
                    DynamicsGroupingTitle = "ПО НЕДЕЛЯМ";
                    var current = minDate;
                    while (current <= maxDate)
                    {
                        var weekEnd = current.AddDays(6);
                        if (weekEnd > maxDate) weekEnd = maxDate;

                        labels.Add($"{current:dd.MM}"); 

                        var chunk = currentItems.Where(x => x.Date.Date >= current && x.Date.Date <= weekEnd).ToList();

                        incomeData.Add(chunk.Where(i => i.OperationType == OperacionTyps.ZACHISLENIE).Sum(i => i.Sum));
                        expenseData.Add(chunk.Where(i => i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(i => i.Sum));

                        current = current.AddDays(7);
                    }
                }
                else // Менее 3 недель -> По дням
                {
                    DynamicsGroupingTitle = "ПО ДНЯМ";

                    for (DateTime date = minDate; date <= maxDate; date = date.AddDays(1))
                    {
                        labels.Add(date.ToString("dd.MM"));

                        double income = currentItems.Where(i => i.Date.Date == date && i.OperationType == OperacionTyps.ZACHISLENIE).Sum(x => x.Sum);
                        double expense = currentItems.Where(i => i.Date.Date == date && i.OperationType != OperacionTyps.ZACHISLENIE && i.OperationType != OperacionTyps.UNREACHABLE).Sum(x => x.Sum);

                        incomeData.Add(income);
                        expenseData.Add(expense);
                    }
                }

                dynamicsStats.DailyChartLabels = labels.ToArray();
                dynamicsStats.DailyIncomeData = incomeData.ToArray();
                dynamicsStats.DailyExpenseData = expenseData.ToArray();

                // Calculate "Nice" Max for Y-Axis
                double maxVal = 0;
                if (expenseData.Any()) maxVal = expenseData.Max();
                if (incomeData.Any()) maxVal = Math.Max(maxVal, incomeData.Max());

                if (maxVal > 0)
                {
                    // Round up to nice number
                    double magnitude = Math.Pow(10, Math.Floor(Math.Log10(maxVal)));
                    double normalized = maxVal / magnitude;
                    
                    double niceNormalized;
                    if (normalized <= 1.0) niceNormalized = 1.0;
                    else if (normalized <= 2.0) niceNormalized = 2.0;
                    else if (normalized <= 5.0) niceNormalized = 5.0;
                    else niceNormalized = 10.0;

                    ChartYAxisMax = niceNormalized * magnitude;
                }
                else
                {
                    ChartYAxisMax = 1000; // Default
                }
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
                    .Where(x => !string.IsNullOrEmpty(x.Description))
                    .GroupBy(x => x.Description)
                    .Select(g => new CategoryStat
                    {
                        Name = g.Key!,
                        Amount = g.Sum(x => x.Sum),
                        Percentage = (g.Sum(x => x.Sum) / totalForCalc) * 100,
                        IsExpense = !(g.Select(x=>x.OperationType).FirstOrDefault() is OperacionTyps.ZACHISLENIE)
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
                var configManager = EfcToXamarinAndroid.Core.Configs.ManagerCore.ConfigurationManager.ConfigManager;
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
                var downloadsPath = await _fileService.PickFolderAsync();
                if (string.IsNullOrEmpty(downloadsPath))
                {
                    await _uiService.ShowToastAsync("Экспорт отменен.");
                    return;
                }

                var fileName = $"FinReport{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.xml";
                var filePath = System.IO.Path.Combine(downloadsPath, fileName);

                var success = await _dataService.SaveDataItemsToXmlAsync(filePath);

                if (success)
                {
                    await _uiService.ShowToastAsync($"Данные экспортированы в {fileName}");
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
                    if (dataItems != null && dataItems.Any())
                    {
                        var success = await DatesRepositorio.AddDatas(dataItems.ToList());
                        if (success)
                        {
                            await RefreshData();
                            await _uiService.ShowToastAsync($"Импортировано {dataItems.Count} записей. Всего: {AllItems.Count}");
                        }
                        else
                        {
                            await _uiService.ShowToastAsync("Ошибка: Не удалось сохранить данные в базу (БД заблокирована или ошибка записи).");
                        }
                    }
                    else if (dataItems != null)
                    {
                         await _uiService.ShowToastAsync("Файл прочитан, но записей не найдено (возможно, пустой или дубликаты).");
                    }
                    else
                    {
                         await _uiService.ShowToastAsync("Ошибка: Не удалось парсить файл (неверный формат).");
                    }
                }
                else
                {
                    await _uiService.ShowToastAsync("Импорт отменен.");
                }
            }
            catch (Exception ex)
            {
                await _uiService.ShowToastAsync($"Ошибка импорта данных: {ex.Message}");
            }
        }

        public async Task ImportPdfAsync()
        {
            try
            {
                var filePath = await _fileService.PickFileAsync("Выберите PDF файл для импорта", [".pdf"]);
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ProcessPdfFileAsync(filePath);
                    await RefreshData();
                }
                else
                {
                    await _uiService.ShowToastAsync("Импорт отменен.");
                }
            }
            catch (Exception ex)
            {
                await _uiService.ShowToastAsync("Ошибка импорта PDF.");
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
        public async Task UpdateItemValueAsync(int id, FinanceItem item, Receipt? receipt = null, string? pendingQrCode = null)
        {
            DataItem dataItem = new DataItem(item.OperationType, item.Date);

            dataItem.Sum = item.Sum;
            dataItem.Descripton = item.Description;
            dataItem.Title = item.Title;
            dataItem.MccDeskription = item.MccDescription;
            dataItem.MCC = item.MCC;
            dataItem.UnreachableText = item.UnreachableText;
            dataItem.IsNewDataItem = item.IsNewDataItem;
            dataItem.Balance = item.Balance;
            dataItem.PendingQrCode = pendingQrCode; // Сохраняем QR-код для повторной проверки

            int dataItemId = id;

            if (id == 0)
            {
                // Note: AddDatas works with a list and internally updates items... 
                // But AddDatas implementation in Repositorio is a bit complex with dedup.
                // However, the 'newDataItems' list handling suggests it updates specific properties.
                // The critical part is obtaining the ID of the newly inserted item. 
                // DatesRepositorio.AddDatas does NOT return the ID directly easily.

                // Workaround: We set a unique temporary ID (HashId) or use the object reference if possible, 
                // but AddDatas creates NEW objects. 

                // Better approach: We rely on the fact that AddDatas will add it to DataItems list. 
                // We can find it by HashId or similar properties.
                if (receipt != null)
                {
                    dataItem.Date = receipt.ReceiptDate;
                }
                await DatesRepositorio.AddDatas(new List<DataItem> { dataItem });
                await RefreshData();
                
                // Find the item we just added. 
                // Assumptions: exact Sum, Date, Description match.
                var addedItem = DatesRepositorio.DataItems
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault(x => 
                        x.Sum == dataItem.Sum && 
                        x.Date == dataItem.Date &&
                        x.Descripton == dataItem.Descripton);
                        
                if (addedItem != null)
                {
                    dataItemId = addedItem.Id;
                }
            }
            else
            {
                await DatesRepositorio.UpdateItemValue(id, dataItem);
            }

            if (receipt != null && dataItemId != 0)
            {
                receipt.DataItemId = dataItemId;
                await DatesRepositorio.SaveReceiptAsync(receipt);
            }
        }

        public async Task<bool> RetryReceiptAsync(FinanceItem item)
        {
            if (string.IsNullOrEmpty(item.PendingQrCode)) return false;

            try 
            {
                var data = EfcToXamarinAndroid.Core.Parsers.FnsQrParser.Parse(item.PendingQrCode);
                if (string.IsNullOrEmpty(data.TransactionCode)) return false;

                // Use date from QR or from item
                var dateToUse = data.Date ?? item.Date;
                
                var receipt = await _qrCodeService.GetReceiptByTransactionCodeAsync(dateToUse, data.TransactionCode);
                if (receipt != null)
                {
                    // Update item properties from receipt
                    item.Sum = receipt.TotalSum;
                    item.Description = receipt.Subject;
                    item.PendingQrCode = null; // Clear pending code
                    
                    // Save changes to DataItem
                    await UpdateItemValueAsync(item.Id, item, receipt, null);
                    await RefreshData();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RetryReceiptAsync Error: {ex.Message}");
            }
            return false;
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
                Balance = item.Balance,
                PendingQrCode = item.PendingQrCode
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
            item.PendingQrCode = oldItem.PendingQrCode;

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

        public int GetMccCodesByDeskription(string type)
        {
            var deskr = AllItems.FirstOrDefault(x => x.MccDescription == type);
            if (deskr != null)
                return deskr.MCC;
            else
               return MccCodes.FirstOrDefault(x => x.Value == type).Key;
        }
        public string GetMccDeskriptonsByCode(int type)
        {
            var deskr = AllItems.FirstOrDefault(x => x.MCC == type);
            if (deskr!=null)
                return deskr.MccDescription??string.Empty;
            else
                return MccCodes.FirstOrDefault(x => x.Key == type).Value;
        }

        private List<FinanceItem> _currentFilteredList = new();

        public async ValueTask<ItemsProviderResult<FinanceItem>> GetFinanceItemsProvider(ItemsProviderRequest request, OperacionTyps filterType)
        {
            // Если тип фильтра изменился, или кэш пуст (первый запуск), или запрос сдвигается за пределы (хотя лист в памяти полный)
            // на самом деле MainLayout/Counter вызывает GetActiveItems() при смене таба или фильтра, 
            // так что _currentFilteredList должен быть уже актуальным.
            
            // Но для надежности, если вдруг ItemsProvider вызвался раньше или без обновления:
            if (_currentFilteredList == null || CurentType != filterType)
            {
                 // Это вызовет пересчет статистики, если таб сменился "неявно" (что вряд ли)
                 _currentFilteredList = GetFilteredItems(filterType).ToList();
            }

            var totalCount = _currentFilteredList.Count;
            var pagedItems = _currentFilteredList.Skip(request.StartIndex).Take(request.Count).ToList();

            return new ItemsProviderResult<FinanceItem>(pagedItems, totalCount);
        }

        /// <summary>
        /// Освобождает ресурсы и отписывается от событий, чтобы предотвратить утечки памяти.
        /// </summary>
        public void Dispose()
        {
            _smsReader.SmsReceived -= _smsReader_SmsReceived;
            
            DatesRepositorio.PaymentsChanged -= OnDataChanged;
            DatesRepositorio.DepositsChanged -= OnDataChanged;
            DatesRepositorio.CashsChanged -= OnDataChanged;
            DatesRepositorio.UnreachableChanged -= OnDataChanged;
        }

        private void ApplyDefaultPeriod()
        {
            var period = _appConfiguration.DefaultPeriod;
            DateTime end = DateTime.Now.Date;
            DateTime start = end;

            if (period == DisplayPeriod.Auto)
            {
                // Auto Logic
                if (!AllItems.Any())
                {
                    period = DisplayPeriod.Week; // Fallback for new users
                }
                else
                {
                    var firstDate = AllItems.Min(x => x.Date).Date;
                    var totalDays = (end - firstDate).TotalDays;

                    // "When it's possible to compare months" -> roughly > 2 months (60 days)
                    if (totalDays < 60)
                    {
                        period = DisplayPeriod.Week;
                    }
                    else if (totalDays < 180) // < 6 months
                    {
                        period = DisplayPeriod.Month;
                    }
                    else if (totalDays < 365) // < 1 year
                    {
                        period = DisplayPeriod.HalfYear;
                    }
                    else
                    {
                        period = DisplayPeriod.Year;
                    }
                }
            }

            switch (period)
            {
                case DisplayPeriod.Week:
                    start = end.AddDays(-6); // Last 7 days including today
                    break;
                case DisplayPeriod.Month:
                    start = new DateTime(end.Year, end.Month, 1);
                    break;
                case DisplayPeriod.HalfYear:
                    start = end.AddMonths(-6);
                    break;
                case DisplayPeriod.Year:
                    start = new DateTime(end.Year, 1, 1);
                    break;
                case DisplayPeriod.All:
                    SetDateRange(null);
                    return;
                case DisplayPeriod.Auto: // Should be resolved above, but needed for compiler/fallback
                    start = end.AddDays(-6);
                    break;
            }

            SetDateRange(new DateRange(start, end));
        }
    }
}


