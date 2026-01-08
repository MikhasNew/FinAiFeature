using System;
using EfcToXamarinAndroid.Core; // Предполагаю наличие enum OperacionTyps здесь

namespace EfcToXamarinAndroid.UI.Components.Models
{
    public class FinanceItem
    {
        // ==========================================
        // Исходные данные (Raw Data)
        // ==========================================

        // Используем backing fields для свойств, изменение которых влияет на UI
        private DateTime _date;
        private float _sum;
        private OperacionTyps _operationType;
        private string? _description;
        private string? _mccDescription;


        public int Id { get; set; }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    // Сбрасываем кеш зависимых свойств
                    _formattedDate = null;
                    _formattedTime = null;
                    _shortFormattedLabel = null;
                }
            }
        }

        public float Sum
        {
            get => _sum;
            set
            {
                if (Math.Abs(_sum - value) > 0.001f)
                {
                    _sum = value;
                    _formattedSum = null; // Сброс кеша
                    // Цвет пересчитывается дешево, но можно тоже кешировать логику
                }
            }
        }

        public OperacionTyps OperationType
        {
            get => _operationType;
            set
            {
                if (_operationType != value)
                {
                    _operationType = value;
                    _displayIcon = null; // Сброс иконки
                    _formattedSum = null; // Сумма зависит от типа (знак +/-)
                }
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                _displayTitle = null;
            }
        }

        public string? MccDescription
        {
            get => _mccDescription;
            set
            {
                _mccDescription = value;
                _displayDescription = null;
            }
        }

        // Остальные простые свойства
        public string? Title { get; set; }
        public string? Icon { get; set; }
        public int MCC { get; set; }
        public string? UnreachableText { get; set; }
        public bool IsNewDataItem { get; set; }
        public float Balance { get; set; }

        // Бесполезное свойство для биндинга (лучше убрать, если не критично для XAML хаков)
        public FinanceItem ThisFinanceItem => this;

        // ==========================================
        // ОПТИМИЗИРОВАННЫЕ UI СВОЙСТВА (Lazy Loading)
        // ==========================================

        // 1. Форматированная сумма
        private string? _formattedSum;
        public string FormattedSum
        {
            get
            {
                if (_formattedSum == null)
                {
                    CalculateSumAndColor();
                }
                return _formattedSum!;
            }
        }

        // 2. Цвет суммы
        private string? _amountColor;
        public string AmountColor
        {
            get
            {
                if (_amountColor == null)
                {
                    CalculateSumAndColor();
                }
                return _amountColor!;
            }
        }

        private void CalculateSumAndColor()
        {
            // Логика вынесена в один метод. 
            // Сравнение Enum напрямую (быстро), без ToString()
            bool isExpense = OperationType == OperacionTyps.OPLATA ||
                             OperationType == OperacionTyps.NALICHNYE;

            if (isExpense)
            {
                float displaySum = _sum > 0 ? -_sum : _sum;
                _amountColor = "red";
                _formattedSum = displaySum.ToString("F2"); // Создаем строку 1 раз
            }
            else
            {
                float displaySum = _sum < 0 ? -_sum : _sum;
                _amountColor = "green";
                _formattedSum = $"+{displaySum:F2}";
            }
        }

        // 3. Дата и Время
        private string? _formattedDate;
        public string FormattedDate => _formattedDate ??= _date.ToString("dd.MM.yyyy");

        private string? _formattedTime;
        public string FormattedTime => _formattedTime ??= _date.ToString("HH:mm:ss");

        // 4. Заголовки (Упрощенная логика)
        private string? _displayTitle;
        public string DisplayTitle => _displayTitle ??= string.IsNullOrEmpty(_description) ? "Операция" : _description!;

        private string? _displayDescription;
        public string DisplayDescription => _displayDescription ??= string.IsNullOrEmpty(_mccDescription) ? "Без описания" : _mccDescription!;

        // 5. Иконка
        private string? _displayIcon;
        public string DisplayIcon
        {
            get
            {
                if (_displayIcon == null)
                {
                    // SWITCH по Enum, а не по строке! Это в 100 раз быстрее.
                    _displayIcon = OperationType switch
                    {
                        OperacionTyps.OPLATA => "@Icons.Material.Filled.Payment",
                        OperacionTyps.ZACHISLENIE => "@Icons.Material.Filled.AccountBalance",
                        OperacionTyps.NALICHNYE => "@Icons.Material.Filled.AttachMoney",
                        OperacionTyps.UNREACHABLE => "@Icons.Material.Filled.QuestionMark",
                        _ => "@Icons.Material.Filled.Payment"
                    };
                }
                return _displayIcon;
            }
        }

        // 6. Кеширование для метода с параметром
        public static int CurrentYear = DateTime.Now.Year;
        private string? _shortFormattedLabel;
      
        public string ShortFormattedLabel
        {
            get
            {
                // ОПТИМИЗАЦИЯ: Если строка уже сформирована, возвращаем её сразу (быстрее не бывает)
                if (_shortFormattedLabel != null)
                {
                    return _shortFormattedLabel;
                }

                // ЛОГИКА: Формируем строку только один раз, используя общее статическое значение
                if (_date.Year == CurrentYear)
                {
                    _shortFormattedLabel = $"{_date:HH:mm} · {_date:dd.MM}";
                }
                else
                {
                    _shortFormattedLabel = $"{_date:HH:mm} · {_date:dd.MM.yyyy}";
                }

                return _shortFormattedLabel;
            }
        }
    }
}